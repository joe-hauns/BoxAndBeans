function [FigHandle] = plot_3d_prototype_model(X, Y, W, Y_W, Omega, Fig)
% Plots a 3D prototype model with optional relevance projection.
%
% Let N be the number of data points and K be the number of prototypes.
%
% REQUIRED INPUT: 
%   X       ... An N x 3 matrix of data, where each row is one data vector.
%   Y       ... (optional) A N x 1 vector of labels, where Y(i) is the
%               label for data point X(i, :)
%   W       ... (optional) A K x 3 matrix of prototypes, where each row is
%               one prototype vector.
%   Y_W     ... (optional) A K x 1 vector of labels, where Y_W(k) is the
%               label for prototype W(k, :)
%   Omega   ... (optional) A 3 x 3 relevance matrix. This matrix can also
%               be higher-dimensional, in which case a discriminative
%               dimensionality reduction (a PCA on the relevance matrix) is
%               applied to handle higher-dimensional data.
%
% OPTIONAL INPUT:
%   Fig ... either a string as the title for the new figure window or a
%           figure handle in which case no new figure window will be
%           created.
%
% OUTPUT:
%   FigHandle ... a Matlab handle (i.e. address) of the figure window

% check data
if(nargin < 1 || isempty(X) || ~ismatrix(X))
    error('expected a data matrix as first argument');
end

N = size(X, 1);
d = size(X, 2);

if(d < 3)
    X = [X, zeros(N, 3 - d)];
end

% check labels
if(nargin >= 2 && ~isempty(Y))
    if(~isvector(Y) || numel(Y) ~= N)
        error('Expected one label per data point!');
    end
else
    Y = inf(N, 1);
end

% check prototypes
has_prototypes = nargin >= 3 && ~isempty(W);
if(has_prototypes)
    if(~ismatrix(W))
        error('Expected a prototype matrix as third argument!');
    end
    K = size(W, 1);
    if(size(W, 2) ~= d)
        error('Data and prototypes do not have the same dimensionality!');
    end
    if(d == 1)
        W = [W, zeros(K, 1)];
    end

    % check prototype labels
    if(nargin >= 4 && ~isempty(Y_W))
        if(~isvector(Y_W) || numel(Y_W) ~= K)
            error('Expected one label per prototype!');
        end
    else
        Y_W = inf(K, 1);
    end

    % prepare Omega to do relevance projection if necessary
    if(nargin >= 5 && ~isempty(Omega))
        % check size of omega
        if(~ismatrix(Omega) || size(Omega, 1) ~= d || size(Omega, 2) ~= d)
            error('Omega is not a square matrix acc. to data dimensionality!');
        end
        % check if relevance projection is required
        if(d > 3)
            Omega = extract_relevance_projection(Omega' * Omega, 3)';
        end
        % project data and prototypes according to Omega.
        X = X * Omega';
        W = W * Omega';
    else
        if(d > 3)
            error('Expected 3-dimensional input data (or relevance matrices for relevance projection)!');
        end
    end
end

d = 3;

if nargin>=6 && ~isempty(Fig)
    if(ischar(Fig))
        FigHandle = figure('Name', Fig);
    else
        FigHandle = Fig;
        figure(Fig);
    end
else
    % if no FigTitle is given, use default title
    FigHandle = figure('Name', '2D data and prototypes');
end

% Start the actual plotting

cm = tango_color_scheme;  % use the tango color map.

uniqueLabels = unique(Y);
L            = numel(uniqueLabels);

for l=1:L  % iterate through data classes
    % compute color index
	c       = mod(l - 1, size(cm, 1)) + 1;
    inClass = Y == uniqueLabels(l);
    % plot data in 2D
    plot3( X(inClass, 1), X(inClass, 2), X(inClass, 3), 'LineStyle', 'none', 'Marker', 'o', 'MarkerSize', 5, 'MarkerEdgeColor', squeeze(cm(c,3,:)), 'MarkerFaceColor', squeeze(cm(c,1,:)) );
    hold on;  % hold figure, so everything is plotted in addition to existing
    if(has_prototypes)
        % plot prototypes in 2D
        inClass = Y_W == uniqueLabels(l);
        plot3( W(inClass, 1), W(inClass, 2), W(inClass, 3), 'LineStyle', 'none', 'Marker', 'd', 'MarkerSize', 10, 'MarkerEdgeColor', squeeze(cm(c,3,:)), 'MarkerFaceColor', squeeze(cm(c,1,:)), 'LineWidth', 2);
    end
end
if(has_prototypes)
    % plot remaining prototypes
    uniqueLabels = setdiff(unique(Y_W), uniqueLabels);
    for l=1:numel(uniqueLabels)  % iterate through data classes
        % compute color index
        c       = mod(L + l, size(cm, 1)) + 1;
        % plot prototypes in 2D
        inClass = Y_W == uniqueLabels(l);
        plot3( W(inClass, 1), W(inClass, 2), W(inClass, 3), 'LineStyle', 'none', 'Marker', 'd', 'MarkerSize', 10, 'MarkerEdgeColor', squeeze(cm(c,3,:)), 'MarkerFaceColor', squeeze(cm(c,1,:)), 'LineWidth', 2);
    end
end
hold off;

axis tight;  % set coordinate system to fit the data
axis equal;