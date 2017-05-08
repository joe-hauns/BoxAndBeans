%% Some initial settings 
clear all; close all; clc;
%% generate the data

nSamples = 1000;
nChan    = 8;
nOuts    = 3;
X = zeros(nSamples , nChan);
Y = zeros(nSamples , nOuts);

for n = 1:nOuts
    freq    = 0.02*pi;
    Y(:, n) = sin(freq * (1:nSamples) + (2*n)/(3*freq));
    Y(Y(:,n) < 0, n) = 0;
end

for n = 1:nChan
    freq   = 0.5 + randn*0.01;
    
    if n <=3 
        ampl = Y(:,1) + 1;
    elseif n <=6
        ampl = Y(:,2) + 1;
    else
        ampl = Y(:,3) + 1;
    end
    
    X(:,n) = ampl.*sin(freq * (1:nSamples)') + randn(nSamples, 1) * 0.1;
end

figure; hold on;
plot(1:nSamples, X(:,1), 'b');
plot(1:nSamples, Y(:,1), 'g'); 
hold off;

%% initialize an ESN
woSteps = 20;
N = 500;

lspecs = {'class','ESN'; 'hidDim',N; 'reg',1e-7; 'woSteps',woSteps; 'inpDim',8; 'outDim',3; 'dt', 0.25; 'specRad', 0.9};
esn = ESN(8, 3, lspecs);
esn.init();

esn.train(X, Y);

%% apply the ESN

Y_apply = esn.apply(X);

%% plot the results
colors = tango_color_scheme();

figure;
hold on;
for dof=1:3
    plot(1:nSamples, Y_apply(:, dof), 'Color', squeeze(colors(dof, 3, :)));
    plot(1:nSamples, Y(:, dof), 'Color', squeeze(colors(dof, 1, :)));
end
hold off;


