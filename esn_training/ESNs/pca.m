function [eigvalues eigvectors coeffs mX] = pca(X, dim)
%PCA Summary of this function goes here
%   [eigvalues eigvectors coeffs mX] = pca(X, dim)

numSamples = size(X,1);

%remove mean
mX = mean(X,1);
X = X - repmat(mX, numSamples, 1);

%calc covariance matrix
C = X'*X;

%calc eigenvectors and values
[v, e] = eig(C);

%sort eigenvalues and vectors
% [eigvalues, indices] = sort(diag(e), 'descend');
% eigvectors = v(:,indices);
eigvalues = flipdim(diag(e),1); % eig returns everything in ascending order
eigvectors = flipdim(v,2);

%conduct transformation into pca-space using dim components
coeffs = X * eigvectors(:,1:dim);

end

