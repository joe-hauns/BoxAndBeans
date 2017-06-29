function plot_multiple_errorbars( x, errors, method_labels )
% plots multiple errorbar line plots for the given x, the given error table
% and the given method labels.
% We expect:
% x             - an N x 1 vector, where N is the number of test conditions
% errors        - an F x N x M tensor, where F is the number of
%                 crossvalidation folds, M is the number of methods/the
%                 number of plots to generate and N is the number of test
%                 conditions
% method_labels - an M x 1 cell array with labels for each method

if(isempty(x) || ~isvector(x))
    error('x is no vector');
end
if(size(x, 2) ~= 1)
    x = x';
end
N = numel(x);

if(isempty(method_labels) || ~iscell(method_labels))
    error('expected method_labels to be a M x 1 cell array');
end
M = numel(method_labels);


if(isempty(errors) || numel(size(errors))~=3 || size(errors, 2) ~= N || size(errors, 3) ~= M)
    error('expected errors to be a F x N x M tensor');
end

colors = tango_color_scheme();

hold on;
for m=1:M
    c = mod(m - 1, size(colors, 1)) + 1;
    if(size(errors, 1) > 1)
        errorbar(x, squeeze(mean(errors(:, :, m))), squeeze(std(errors(:, :, m))), 'Color', squeeze(colors(c,3,:)));
    else
        plot(x, squeeze(errors(:, :, m)), 'Color', squeeze(colors(c,3,:)));
    end
end
hold off;
legend(method_labels);
ylabel('avg. error');
grid on;

end

