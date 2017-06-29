% demo memory capacity
% compute the memory capacity on a 1D signal as in
% Jaeger: Short term memory in echo state networks, 2002

%% generate the input
seqL_tr = 200;
seqL_te = 200;
seqL    = seqL_tr + seqL_te;
nRep    = 10;

Xtmp = 2*rand(round(seqL/nRep), 1) -1;
% copy each value 10 times to reproduce jaergers scenario
X = zeros(seqL,1);
count = 0;
for i=1:nRep:seqL
    count = count+1;
    X(i:i+nRep-1) = repmat(Xtmp(count), nRep, 1);
end

X_tr = X(1:seqL_tr);
X_te = X(seqL_tr+1:seqL_tr+seqL_te);

%% generate the target, i.e. delay the input
K = 40;
Y = zeros(seqL,K);
for k=1:K
    Y(1:k,k)     = 2*rand(k, 1) -1;
    Y(k+1:end,k) = X(1:end-k);
end

Y_tr = Y(1:seqL_tr, :);
Y_te = Y(seqL_tr+1:seqL_tr+seqL_te, :);


%% apply the ESN model
resS    = 20;
reg     = 0.00000;
woSteps = 100;
dimIn   = 1;
dimOut  = K;
dtCons  = 1; % no leaky rate as in jaegers paper
specRad = 0.90;
univRes = 0;
linESN  = 0;
psdRes  = 1;
useRelu = 0;
lspecs  = {'class','ESN'; 'hidDim',resS; 'reg',reg; 'inpScale', 1; ...
        'woSteps',woSteps; 'inpDim',dimIn; 'outDim',dimOut; 'psdRes', psdRes; ...
        'dt', dtCons; 'specRad', specRad; 'universRes', univRes; ...
        'linearESN', linESN; 'useReLU',useRelu; 'use_init_h', 0}; %; ...
        %'universResPars', [v,rc,rj,jumps]; };

esn = ESN(dimIn, dimOut, lspecs);
esn.init();
esn.train(X_tr, Y_tr);


preds_te = esn.apply(X_te);
preds_te = preds_te(woSteps+1:end,:);
Y_teEval = Y_te(woSteps+1:end,:);

preds_tr = esn.apply(X_tr);
preds_tr = preds_tr(woSteps+1:end,:);
Y_trEval = Y_tr(woSteps+1:end,:);

corrSq_tr = zeros(K,1);
for k=1:K
    corrSq_tr(k) = ...
        mean((preds_tr(:,k) - mean(preds_tr(:,k))) .* (Y_trEval(:,k) - mean(Y_trEval(:,k))))/ ...
        (std(preds_tr(:,k)) * std(Y_trEval(:,k)));
    corrSq_tr(k) = corrSq_tr(k)^2;
end

corrSq = zeros(K,1);
for k=1:K
    corrSq(k) = ...
        mean((preds_te(:,k) - mean(preds_te(:,k))) .* (Y_teEval(:,k) - mean(Y_teEval(:,k))))/ ...
        (std(preds_te(:,k)) * std(Y_teEval(:,k)));
    corrSq(k) = corrSq(k)^2;
end
MC = sum(corrSq);

myFigure; subplot(1,2,1);
plot(corrSq_tr); 
title(['trainingm MC: ' num2str(sum(corrSq_tr))]); axis([0 K 0 1]);
subplot(1,2,2); plot(corrSq); 
title(['testing, MC: ' num2str(MC)]); axis([0 K 0 1]);

% % plot example predictions
% k = 7;
% myFigure; hold on;
% plot(1:size(Y_teEval,1), Y_teEval(:,k), 'b');
% plot(1:size(preds_te,1), preds_te(:,k), 'g');
% 
% myFigure; hold on;
% plot(1:size(Y_trEval,1), Y_trEval(:,k), 'b');
% plot(1:size(preds_tr,1), preds_tr(:,k), 'g');





