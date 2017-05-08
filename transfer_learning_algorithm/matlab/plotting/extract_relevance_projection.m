function [ Proj ] = extract_relevance_projection( Lambda, dims )
% Does an Eigenvalue decomposition of the given Lambda matrix and builds a
% projection matrix using the dims most important Eigenvectors.
% Input parameters:
% Lambda - a m x m relevance matrix, as devised by metric learning.
% dims   - the desired number of output dimensions n < m.
% Output:
% Proj   - a m x n projection matrix mapping the input dimensionality to
%          the output dimensionality.

m = size(Lambda, 1);

if(~ismatrix(Lambda) || size(Lambda, 2) ~= m)
    error('Malformed Lambda. Expected square matrix!');
end

if(nargin < 2 || isempty(dims))
    dims = 2;
end

if(~isscalar(dims) || dims > m)
    warning('Malformed dimensionality. Using 2 per default');
    dims = 2;
end

[u,v]   = eig(Lambda);
v       = diag(v);
% sort the eigenvalues in ascending order
[v,idx] = sort(v);
u       = u(:,idx);
% use the last dims eigenvectors
idx     = m-dims+1:m;

Proj    = u(:,idx) * diag(sqrt(v(idx)));


end

