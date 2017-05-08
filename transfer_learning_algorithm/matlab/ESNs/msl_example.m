%Example for learning in the model space
%The goal is to differentiate between two noisy sine waves

length = 10000;
sample_length = 100;
noise_std = 0.1;
n_samples = 2*length/sample_length
N = 30;

rng(42);
t=[0:length]';
sin1=sin(0.2*t)+noise_std*randn(length+1,1);
sin2=sin(0.311*t)+noise_std*randn(length+1,1);

X_mat = zeros(n_samples,sample_length);
X = cell(n_samples,1);
Y = cell(n_samples,1);
ind = 1;
for i=1:sample_length:length
    X{ind} = sin1(i:i+sample_length-1);
    X_mat(ind,:) = X{ind};
    Y{ind} = sin1(i+1:i+sample_length);
    ind = ind + 1;
    X{ind} = sin2(i:i+sample_length-1);
    X_mat(ind,:) = X{ind};
    Y{ind} = sin2(i+1:i+sample_length);
    ind = ind + 1;
end
sine1_ind = 1:2:n_samples;
sine2_ind = 2:2:n_samples;


% figure
% plot(X{1})
% figure;
% plot(Y{1})

lspecs = {'class','ESN'; 'hidDim',N; 'reg',1; 'woSteps',5; 'inpDim',1; 'outDim',1};
%% create learner
rng(42);
learner = ESN(1, 1, lspecs);
%cmd = ['learner = ' lspecs{1,2} '(inpDim, outDim, lspecs);'];
%eval(cmd);
learner.init(X(1:2));

%% create models
models = zeros(n_samples,N);
for i=1:n_samples
    learner.h = zeros(N,1);
    learner.train(X(i), Y(i));
    models(i,:) = reshape(learner.wOut, 1, numel(learner.wOut));
end

figure('position', [300, 150, 1000, 800])
subplot(2,2,1:2)
plot(sin1(1:500), 'b');
hold on;
plot(sin2(1:500), 'g');
title('Signals');
legend('sin(0.2)', 'sin(0.311)');

subplot(2,2,3)
[eigvalues eigvectors repr mX]= pca(X_mat, 2); 
hold on;
plot(repr(sine1_ind,1), repr(sine1_ind,2), 'bo')
plot(repr(sine2_ind,1), repr(sine2_ind,2), 'go')
legend('sin(0.2)', 'sin(0.311)');
title('Signal Space')
xlabel('pc1'); ylabel('pc2');

subplot(2,2,4)
[eigvalues eigvectors repr mX]= pca(models, 2); 
plot(repr(sine1_ind,1), repr(sine1_ind,2), 'bo')
hold on;
plot(repr(sine2_ind,1), repr(sine2_ind,2), 'go')
legend('sin(0.2)', 'sin(0.311)');
title('Model Space')
xlabel('pc1'); ylabel('pc2');
hold off;
