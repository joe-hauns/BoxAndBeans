% test ESN on the adding problem ... ESNs do not work well here
% generate the data
seqL    = 40;
numS_tr = 10000;
numS_te = 1000;

X = cell(numS_tr+numS_te,1);
Y = cell(numS_tr+numS_te,1);
for s = 1:(numS_tr+numS_te)
    X{s} = rand(seqL,1);
    X{s} = [X{s}, zeros(seqL,1)];
    % select two random entries
    randIdx = randperm(seqL);
    randIdx = randIdx(1:2);
    X{s}(randIdx,2) = 1;
    
    Y{s} = repmat(sum(X{s}(randIdx,1)), seqL, 1);
end

X_tr = X(1:numS_tr);
X_te = X(numS_tr+1:numS_tr+numS_te);
Y_tr = Y(1:numS_tr);
Y_te = Y(numS_tr+1:numS_tr+numS_te);

%% apply the ESN model
resS    = 200;
reg     = 0.01;
woSteps = 1; % dont do washout since the inportant position can be anywhere
dimIn   = 2;
dimOut  = 1;
dtCons  = 0.4;
specRad = 0.9;
univRes = 0;
linESN  = 0;
useRelu = 0;
lspecs  = {'class','ESN'; 'hidDim',resS; 'reg',reg; ...
        'woSteps',woSteps; 'inpDim',dimIn; 'outDim',dimOut; ...
        'dt', dtCons; 'specRad', specRad; 'universRes', univRes; ...
        'linearESN', linESN; 'useReLU',useRelu; 'use_init_h', 1}; %; ...
        %'universResPars', [v,rc,rj,jumps]; };
                
esn = ESN(dimIn, dimOut, lspecs);
esn.init();
% set the normalization constants
esn.normalizeIO(cell2mat(X_tr), cell2mat(Y_tr));
esn.set_initState([rand(seqL,1) zeros(seqL,1)]);
esn.train(X_tr, Y_tr);

preds_te = esn.apply(X_te);

result_vals = zeros(numS_te,2);
for i=1:numel(X_te)
    result_vals(i,:) = [preds_te{i}(end) Y_te{i}(end)];
end

constMSE = mean((result_vals(:,2) - 1).^2)
esnMSE   = mean((result_vals(:,2) - result_vals(:,1)).^2)





