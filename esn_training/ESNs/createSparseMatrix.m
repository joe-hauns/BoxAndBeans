function [ M ] = createSparseMatrix(rows, cols, density, range)
%CREATESPARSEMATRIX Summary of this function goes here
%   Detailed explanation goes here

if density == 1.0
    M = randu(rows,cols,range);
    return;
end

nonzero = max(0, min(round(density * rows * cols), rows * cols));

M = zeros(rows, cols);
p = randperm(rows * cols);

for i=1:nonzero
    row = ceil(p(i)/cols);
    col = mod(p(i), cols) + 1;
    M(row, col) = randu(1,1,range);
end

M = sparse(M);

end

