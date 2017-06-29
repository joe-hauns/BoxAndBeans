function print_table_with_metadata( metadata, metadata_headers, data, data_headers, idxs, filename, delim )
% Prints the given N x (K + K2) table, where
% metadata         - is a N x K cell array of strings containing metadata
% metadata_headers - is a 1 x K cell array of strings containing the column
%                    headers for metadata
% data             - is a N x K2 matrix containing numeric data
% data_headers     - is a 1 x K2 cell array of strings containing the
%                    column headers for data
% idxs             - is an index vector (logical or numeric indices)
%                    selecting (or ordering) the N rows in the table
%                    (optional; if not provided the original order is used)
% filename         - is the name of a CSV file the data should be written
%                    to (optional; if not provided the table is printed on
%                    the command line)
% delim            - a column delimiter for the table (optional, tab per
%                    default)

% Check the input first
N = size(metadata, 1);
K = size(metadata, 2);

if(~iscell(metadata) || ~ismatrix(metadata))
    error('Expected a N x K cell array as first argument');
end

if(~iscell(metadata_headers) || ~isvector(metadata_headers) || numel(metadata_headers) ~= K)
    error('Expected a 1 x K cell array as first argument');
end

K2 = size(data, 2);

if(~ismatrix(data) || size(data, 1) ~= N)
    error('Expected a N x K2 matrix as third argument');
end

if((~isempty(data_headers) && (~iscell(data_headers) || ~isvector(data_headers))) || numel(data_headers) ~= K2)
    error('Expected a 1 x K2 cell array as fourth argument');
end

if(nargin < 5 || isempty(idxs))
    idxs = 1:N;
else
    if(~isvector(idxs) || max(idxs) > N || (~islogical(idxs) && min(idxs) < 0))
        error('Expected an index vector as fifth argument');
    end
end

if(nargin < 6 || isempty(filename))
    toCmd = true;
else
    toCmd = false;
    fileID = fopen(filename, 'w');
end

if(nargin < 7 || isempty(delim))
    delim = '\t';
end

% retrieve width of metadata columns

metadata_widths = max(cellfun(@(c) (numel(c)), [metadata_headers ; metadata]));

% do the actual printing

% apply the indices first
metadata = metadata(idxs, :);
data = data(idxs, :);

N = size(data, 1);

% print header line first
for k=1:K
    if(k > 1)
        if(toCmd)
            fprintf(delim);
        else
            fprintf(fileID, delim);
        end
    end
    if(toCmd)
        header_formatspec = sprintf('%%%ds', metadata_widths(k));
        fprintf(header_formatspec, metadata_headers{k});
    else
        fprintf(fileID, '%s', metadata_headers{k});
    end
end
clear header_formatspec
for k=1:K2
    if(k > 1 || K > 0)
        if(toCmd)
            fprintf(delim);
        else
            fprintf(fileID, delim);
        end
    end
    if(toCmd)
        fprintf('%s', data_headers{k});
    else
        fprintf(fileID, '%s', data_headers{k});
    end
end
if(toCmd)
    fprintf('\n');
else
    fprintf(fileID, '\n');
end

% Then print data lines
for n=1:N
    % first meta data entries
    for k=1:K
        if(k > 1)
            if(toCmd)
                fprintf(delim);
            else
                fprintf(fileID, delim);
            end
        end
        data_formatspec = sprintf('%%%ds', metadata_widths(k));
        if(toCmd)
            fprintf(data_formatspec, metadata{n, k});
        else
            fprintf(fileID, data_formatspec, metadata{n, k});
        end
    end
    % then data entries
    for k=1:K2
        if(k > 1 || K > 0)
            if(toCmd)
                fprintf(delim);
            else
                fprintf(fileID, delim);
            end
        end
        if(toCmd)
            fprintf('%5.3f', data(n, k));
        else
            fprintf(fileID, '%5.3f', data(n, k));
        end
    end
    if(toCmd)
        fprintf('\n');
    else
        fprintf(fileID, '\n');
    end
end

if(nargin >= 6 && ~isempty(filename))
    fclose(fileID);
end

end

