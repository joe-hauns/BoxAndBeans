function print_table( header, data, filename, delim )
% Writes the given data with the given header as CSV to the given filename

K = size(data, 2);

if(~iscell(header) || ~isvector(header) || numel(header) ~= K)
    error('Expected a 1 x K cell array or K x 1 cell array as first argument');
end

if(nargin < 3 || isempty(filename))
    toCmd = true;
else
    toCmd = false;
    fileID = fopen(filename, 'w');
end

if(nargin < 4 || isempty(delim))
    delim = '\t';
end

for k=1:K
    if(k > 1)
        if(toCmd)
            fprintf(delim);
        else
            fprintf(fileID, delim);
        end
    end
    if(toCmd)
        fprintf('%s', header{k});
    else
        fprintf(fileID, '%s', header{k});
    end
end

if(toCmd)
    fprintf('\n');
else
    fprintf(fileID, '\n');
end

if(~toCmd)
    fclose(fileID);
end

if(~toCmd)
    dlmwrite(filename, data, 'delimiter', delim, '-append');
else
    for i=1:size(data, 1)
        fprintf(['%g', delim] , data(i, :));
        fprintf('\n');
    end
end
end