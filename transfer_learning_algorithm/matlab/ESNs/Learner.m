%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%%          Copyright (c) 2013 R. F. Reinhart, CoR-Lab                 %%%
%%%          Bielefeld University, Germany, http://cor-lab.de           %%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

classdef Learner < handle
    %LEARNER Interface class for function approximator models.
    %   Detailed explanation goes here
    
    properties
        inpDim = 0; %dimension of input
        outDim = 0; %dimension of output
        out = [];
        
        inpNormalization = 1;   % (de)activates normalization of the input
        outNormalization = 1;   % (de)activates normalization of the output
        inpOffset = [];
        inpRange = [];
        outOffset = [];
        outRange = [];
    end
    
    methods
        function l = Learner(inpDim, outDim, spec)
            l.inpDim = inpDim;
            l.outDim = outDim;
            l.out = zeros(1,l.outDim);
            
            if nargin > 2
                for s=1:size(spec,1)
                    if ~strcmp(spec{s,1}, 'class')
                        try
                            l.(spec{s,1}) = spec{s,2};
                        catch me
                            disp(me.message);
                        end
                    end
                end
            end
        end
        
        function [X Y] = normalizeIO(l, X, Y)
            % default values for already normalized data
            l.inpOffset = zeros(1, l.inpDim);
            l.outOffset = zeros(1, l.outDim);
            l.inpRange = repmat([-1; 1], 1, l.inpDim);
            l.outRange = repmat([-1; 1], 1, l.outDim);
            
            if l.inpNormalization && nargin > 1
                [X l.inpOffset l.inpRange] = normalize(X);
            end
            
            if l.outNormalization && nargin > 2
                [Y l.outOffset l.outRange] = normalize(Y);
            end
        end
        
        function init(l, X, Y)
        end
        
        % expect X and Y to be either matrices or cell-arrays of matrices
        function train(l, X, Y)
        end
        
        function Y = apply(l, X)
        end
        
        function new = copy(this)
            new = feval(class(this));
            p = properties(this);
            for i = 1:length(p)
                if( isa(this.(p{i}), 'handle'))
                    new.(p{i}) = this.(p{i}).copy();
                elseif isa(this.(p{i}),'cell')
                    new.(p{i}) = deepCopyCellArray(numel(this.(p{i})));
                else
                    new.(p{i}) = this.(p{i});
                end
            end
        end
    end
    
end

