classdef ESN < Learner
    %ESN Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
    
        hidDim = 10;        % number of neurons in the hidden layer
        wInp = [];          % weights from input layer to hidden layer
        wRes = [];          % weights from hidden layer to hidden layer
        wOut = [];          % weights from hidden layer to output layer
        bias = [];          % bias parameters for activation function
        a = [];             % activation of the hidden layer
        %aNew = [];          % temporal activation, weighted with dt and added to old activation
        h = [];             % set by the act function
        h_old = [];
        y = [];             % is this really needed?
        
        h_init = [];        % initial hidden state. used if use_init_h == 1
        use_init_h = 0;     % variable indicating whether after each 
                            % subsequence the initial state should be
                            % resumed
        
        collectH = 0;       % specifies if the hidden states should be saved
        Hcoll    = [];      % memory for saving hidden states
                            
        % statistics for incremental learning
        HTH = [];           % statistic to store H' * H
        HTY = [];           % statistic to store H' * Y
        
        universRes = 1;     % use Tino's 'Cycle Reservoir with Jumps' (CRJ)
                            % or a standard random reservoir
        universResPars =... % params for the CRJ reservoir: the absolute 
          0.1 * [1 1 1 50]; % magnitude of input weights, the circular and 
                            % jump connection strengths r_c and r_j and the
                            % jump distance between neurons
                            
        linearESN = 0;      % use a linear ESN, i.e. no sigmoid functions
        useReLU   = 0;      % use tanh or a ReLU activation function
        psdRes    = 0;      % use a psd reservoir. good with RelUs?

        dt = 1;             % factor for weighting the contribution of the new reservoir activation
        inpScale = 1;       % scaling for input weights
        biasScale = 0;      % scaling for bias
        inpDensity = 1;     % density of input weights matrix
        resDensity = 0.2;   % density of reservoir weights matrix
        reg = 1e-6;         % regularization parameter for ridge regression
        specRad = 0.95;     % spectral radius for the reservoir weights matrix
        woSteps = 200;      % washout steps applied to the beginning of each sequence
        convergeSteps = 0;  % if >0 the net is fed with the first data vector convergeSteps times before train/apply
        %batchSize = 1000;  % number of samples used for a minibatch
        
    end
    
    methods
        function l = ESN(inpDim, outDim, spec)
            l = l@Learner(inpDim, outDim, spec);
        end

        function updateFromJSON(l, json)
            fields = fieldnames(json.esn);
            for f=1:numel(fields)
                l.(fields{f}) = json.esn.(fields{f});
            end
        end
        
        function init(l, X)  
           if l.universRes == 0
               % use a random reservoir
               if l.inpDensity == 1
                   l.wInp = l.inpScale * (2 * rand(l.hidDim,l.inpDim) - ones(l.hidDim,l.inpDim));
               else
                   l.wInp = createSparseMatrix(l.hidDim, l.inpDim, l.inpDensity, l.inpScale);
               end
               %l.wRes = 2 * rand(l.hidDim,l.hidDim) - ones(l.hidDim,l.hidDim);
               if (l.resDensity > 0)
                   if ~l.psdRes
                       l.wRes = createSparseMatrix(l.hidDim, l.hidDim, l.resDensity, 1);
                   
                   else
                       useVariant = 3;
                       
                       if useVariant == 1
                           % variant 1: compute a scalar product.
                           % It is not sparse
                           l.wRes = createSparseMatrix(l.hidDim, l.hidDim, l.resDensity, 1);
                           % use psd matrices with ReLU activations
                           l.wRes = l.wRes * l.wRes' + eye(l.hidDim);
                       elseif useVariant == 2
                           % variant 2: use sprandsym. Quote from the doc:
                           % It has a great deal of topological and 
                           % algebraic structure.
                           l.wRes = sprandsym(l.hidDim, l.resDensity, (1+rand(l.hidDim,1))/2);
                       elseif useVariant == 3
                           % variant 3: set symmetric random values
                           % there are l.hidDim*(l.hidDim-1)/2 elements in the
                           % upper triangle of a matrix.
                           % set random values for off diagonal elements
                           randPos   = randperm(round(l.hidDim*(l.hidDim-1)/2));
                           nonZeroE  = round(l.resDensity*l.hidDim*(l.hidDim-1)/2);
                           randVals1 = zeros(round(l.hidDim*(l.hidDim-1)/2),1);
                           randVals1(randPos(1:nonZeroE)) = randu(nonZeroE,1);
                           % set random values for off diagonal elements
                           randPos   = randperm(l.hidDim);
                           nonZeroE  = round(l.resDensity*l.hidDim);
                           randVals2 = zeros(l.hidDim,1);
                           randVals2(randPos(1:nonZeroE)) = randu(nonZeroE,1);
                               
                           l.wRes = sparse(squareform(randVals1) + diag(randVals2));
                           l.wRes(1:l.hidDim+1:end) = l.wRes(1:l.hidDim+1:end) ...
                               - eigs(l.wRes, 1,'sa');
                       end
                   end
                   
                   % old scaling
                   %sr     = max(abs(eigs(l.wRes)));
                   %l.wRes = (l.specRad / sr) .* l.wRes;
                   
                   % use the effective spectral radius for leaky integrator
                   sr = max(abs(eigs(l.dt*l.wRes  + (1-l.dt)*eye(l.hidDim))));
                   l.wRes = ((l.specRad -(1-l.dt)) / (sr-(1-l.dt))) .* l.wRes;
               else
                   l.wRes = zeros(l.hidDim);
               end    
           else
               % use Peter Tino's CRJ
               % set the input weights
               currPath   = mfilename('fullpath');
               currPath   = currPath(1:end-3);
               piDecExpan = loadjson([currPath 'pi_8000.json']);
               piDecExpan = piDecExpan.pi8000(1:l.hidDim*l.inpDim);
               
               tmpInp = (-1) * l.universResPars(1) * (piDecExpan < 4.5);
               tmpInp = tmpInp + l.universResPars(1) * (piDecExpan > 4.5);
               l.wInp = reshape(tmpInp, l.hidDim, l.inpDim);
               clear tmpInp piDecExpan;
               
               % set the reservoir weights
               l.wRes = spalloc(l.hidDim, l.hidDim, ...
                   l.hidDim + 2*floor(l.hidDim/l.universResPars(4)));
               % set the circular connection weights
               circ_idx = 2:l.hidDim+1:l.hidDim^2;
               l.wRes(circ_idx) = l.universResPars(2);
               l.wRes(1, l.hidDim) = l.universResPars(2);
               % set the jump weights
               nextToLast = (lower(l.hidDim/l.universResPars(4))-2)*l.universResPars(4) + 1;
               jump_idx   = (1:l.universResPars(4):nextToLast)';
               jump_idx   = [jump_idx+l.universResPars(4) jump_idx];
               if mod(l.hidDim, l.universResPars(4)) == 0
                   jump_idx(end+1, :) = [1 jump_idx(end,1)];
               else
                   jump_idx(end+1, :) = [jump_idx(end,1)+l.universResPars(4) jump_idx(end,1)];
               end
               
               jump_idx1  = (jump_idx(:,2)-1)*l.hidDim + jump_idx(:,1);
               jump_idx2  = (jump_idx(:,1)-1)*l.hidDim + jump_idx(:,2);
               l.wRes(jump_idx1) = l.universResPars(3);
               l.wRes(jump_idx2) = l.universResPars(3);
               clear circ_idx jump_idx jump_idx1 jump_idx2 nextToLast
           end
           
           l.wOut  = zeros(l.hidDim, l.outDim);
           l.bias  = l.biasScale * (2 * rand(l.hidDim,1) - ones(l.hidDim,1));
           l.h     = zeros(l.hidDim,1);
           l.h_old = zeros(l.hidDim,1);
           l.a     = zeros(size(l.h));
           %l.aNew = zeros(size(l.h));
           l.y     = zeros(1,l.outDim);
           
           %not required
           %if nargin > 1
           %     l.normalizeIO(X);
           %end
        end
        
        function train(l, X, Y)
            if iscell(X)
                % check if the input normalization is already set
                if isempty(l.inpOffset)
                    l.normalizeIO(cell2mat(X), cell2mat(Y));
                end
                l.HTH = zeros(l.hidDim, l.hidDim);
                l.HTY = zeros(l.hidDim, l.outDim);                
                numSequences = length(X);
                for i=1:numSequences
                    X{i} = range2norm(X{i}, l.inpRange, l.inpOffset);
                    Y{i} = range2norm(Y{i}, l.outRange, l.outOffset);
                    
                    if l.convergeSteps > 0
                        convergenceInput = repmat(X{i}(1,:), l.convergeSteps, 1);
                        l.calcHiddenStates(convergenceInput);
                    end
                    
                    if l.use_init_h
                        l.h = l.h_init;
                    end
                    H = l.calcHiddenStates(X{i});
                    if l.woSteps > size(X{i},1)
                        warning(['washout longer than sequence ' num2str(i)]);
                        return;
                    end
                    H = H(l.woSteps+1:end,:);
                    l.HTH = l.HTH + H' * H;
                    l.HTY = l.HTY + H' * Y{i}(l.woSteps+1:end,:);
                end
                HTH = l.HTH + diag(repmat(l.reg, 1, l.hidDim));
                l.wOut = HTH\l.HTY;
            else
                % check if the input normalization is already set
                if isempty(l.inpOffset)
                    [X, Y] = l.normalizeIO(X, Y);
                else
                    if l.inpNormalization
                        X = range2norm(X, l.inpRange, l.inpOffset);
                    end
                    if l.outNormalization
                        Y = range2norm(Y, l.outRange, l.outOffset);
                    end
                end
                
                if l.convergeSteps > 0
                    convergenceInput = repmat(X(1,:), l.convergeSteps, 1);
                    l.calcHiddenStates(convergenceInput);
                end
                
                if l.use_init_h
                    l.h = l.h_init;
                end
                numSamples = size(X,1);
                H = l.calcHiddenStates(X);
                
                if l.woSteps > numSamples
                    warning('washout longer than sequence');
                    return;
                end
                Y = Y(l.woSteps+1:end,:);
                H = H(l.woSteps+1:end,:);
                l.HTH  = H' * H;
                l.HTY  = H' * Y;
                l.wOut = (l.HTH + l.reg * eye(l.hidDim))\(l.HTY);
            end
        end
        
        % This function implements an online least squares method. 
        % Before the first run, a manual washout should be performed by
        % executing
        %   l.calcHiddenStates(X);
        % for woSteps number of samples.
        % No forgetting rate is currently implemented.
        function train_online(l, X, Y)
            if isempty(l.HTH) || isempty(l.HTY)
                % if no prior training has been performed (either batch or
                % online): initialize statistics with zeros
                l.HTH = zeros(l.hidDim, l.hidDim);
                l.HTY = zeros(l.hidDim, l.outDim);
            end
            
            if l.inpNormalization
                X = range2norm(X, l.inpRange, l.inpOffset);
            end
            if l.outNormalization
                Y = range2norm(Y, l.outRange, l.outOffset);
            end
            
            
            if l.convergeSteps > 0
                convergenceInput = repmat(X(1,:), l.convergeSteps, 1);
                l.calcHiddenStates(convergenceInput);
            end
            
            % update the statistics
            H = l.calcHiddenStates(X);
            l.HTH = l.HTH + H' * H;
            l.HTY = l.HTY + H' * Y;
            
            % compute the new weights
            l.wOut = (l.HTH + l.reg * eye(l.hidDim))\(l.HTY);
            %l.wOut = l.HTH\(l.HTY);
        end
        
        function [Y] = apply(l, X)
            if iscell(X)
                Y = cell(length(X),1);
                for i=1:length(X)
                    Y{i} = l.apply(X{i});
                end
            else
                if (l.inpNormalization)
                    X = range2norm(X, l.inpRange, l.inpOffset);
                end
                %l.calcHiddenStates(repmat(X(1,:), l.woSteps, 1)); %washout transients
                
                if l.convergeSteps > 0
                    convergenceInput = repmat(X(1,:), l.convergeSteps, 1);
                    l.calcHiddenStates(convergenceInput);
                end
                
                if l.use_init_h
                    l.h = l.h_init;
                end
                H = l.calcHiddenStates(X);
                if l.collectH
                    l.Hcoll = H;
                end
                
                Y = H * l.wOut;
                if (l.outNormalization)
                    Y = norm2range(Y, l.outRange, l.outOffset);
                end
                l.out = Y(end,:);
            end
        end

        % input to calcHiddenStates is always one sequence (steps x dim)
        function [H] = calcHiddenStates(l, X)
            numSamples = size(X,1);
            H = zeros(numSamples, l.hidDim);
            for k=1:numSamples
                l.act(X(k,:));
                H(k,:) = l.h;
            end
        end

        function act(l, x)
            l.a = l.wInp * x' + l.wRes * l.h + l.bias;
            % Hx1 = HxI * (1xI)' + HxH * Hx1 + Hx1
            if l.linearESN
                l.h = l.a;
            else
                % nonlinear activation function
                if l.useReLU
                    l.h = l.a;
                    l.h(l.h<0) = 0;
                else
                    l.h = tanh(l.a);
                end
            end
            
            if l.dt ~= 1
                l.h     = (1-l.dt) * l.h_old + l.dt * l.h;
                l.h_old = l.h;
            end
        end
        
        % If this method is used, the data normalization constants should 
        % be set in advance. Use
        %   l.normalizeIO(X, Y); 
        % for this purpose.
        function set_initState(l, X)
            if l.inpNormalization
                X = range2norm(X, l.inpRange, l.inpOffset);
            end
            H = l.calcHiddenStates(X);
            l.h_init = H(end,:)';
        end
        
        % new function performing a json export of a trained reservoir.
        % str specifies the name of the output
        % if mode == 1, the method will export only the output weights,
        % if mode == 2, everything is exported
        function export(l, str, mode)
            if nargin < 3
                mode = 1;
            end
            
            if mode == 1
                savejson('esn', l.wOut, str);
            elseif mode == 2
                savejson('esn', l, str);
            end
        end

        function reset(l)
           l.h     = zeros(l.hidDim,1);
           l.h_old = zeros(l.hidDim,1);
           l.a     = zeros(size(l.h));
           %l.aNew = zeros(size(l.h));
           l.y     = zeros(1,l.outDim);
        end
    end
    
end

