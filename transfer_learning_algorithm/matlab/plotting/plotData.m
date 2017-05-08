function [fig, plotHandles] = plotData(X, labels, plotAxis, idx, figHandle)
% Plots 2 and 3D data. 
% Required Input args:
%    X              - Input data, Nx2 or Nx3
%    labels         - labels, Nx1
%
% Optional Input args:
%    plotAxis       - plots the axis if 1 (default 0).
%    idx            - plots only that subset of data
%                     data specified in idx (default []).
%    figHandle      - plots into the passed figure
%                     (by default, a new figure is opened).

% Written by Alexander Schulz, Bielefeld University.

% old call:
% function [fig, plotHandles] = plotData(X, labels, idx, figHandle, axisOff)


% transpose the data if necessary
if size(X,2) > 3
    if size(X,1) <= 3
        warning('Transposing the data matrix to have the instances in the rows');
        X = X';
    else
        error('Data should have maximaly 3 features.');
    end
end

if (nargin < 2) || isempty(labels)
    labels = ones(size(X,1),1);
end

if (nargin < 3) || isempty(plotAxis)
    plotAxis = 0;
end

if (nargin >= 4) && (~isempty(idx))
    labels = labels(idx);
    X = X(idx,:);
end

% figures for paper?
forPaper = 0;

if forPaper
    markerSize   = 10;
    fontSizeFigs = 28;
else
    markerSize   = 6;
    fontSizeFigs = 18;
end



% compute number of classes
uniqueLabels = unique(labels);
nClasses     = numel(uniqueLabels);
dim          = size(X,2);

normalLabels = 1;
if ((length(unique(labels)) > 30) || (sum(abs(round(labels) - labels)) > 0))
    % Assuming a regression and plotting continuous!
    % origLabels   = labels;
    normalLabels = 0;
    
    % old, manual variant
%     warning('Assuming a regression and plotting continuous!');
%     %scaleTo = max(1000, length(labels));
%     scaleTo = 1000;
%     labels  = labels - min(labels);
%     scale   = scaleTo/max(labels);
%     labels  = round(labels*scale) + 1;
%     
%     uniqueLabels = unique(labels);
%     nClasses     = scaleTo+1;
end



% select the marker
if normalLabels
    % '.', 'x', '+', are left out
    symbs = {'o', 's', 'd', 'v', '^', '<', '>', 'p', 'h', '*'};
    %symbs = {'o', '*', 'd', 'v', '^', '<', '>', 'p', 'h'}; % tmp
    if nClasses > length(symbs)
        % if more classes then symbols exists, repeat the latter
        symbs = repmat(symbs, 1, ceil(nClasses/length(symbs)));
    end
    %symbs = {'.', 'o'};
else
    % use only one marker
    % symbs = repmat({'o'}, nClasses, 1);
    symbs = 'o';
end

% select the colormap
if normalLabels
    cm = tango_color_scheme;
else
    % cm = jet(nClasses);
    cm = 'hot';
end
%cm = [0.8 0 0; 0 0.8 0; 0 0 0.8]; % for iris data
%cm = [0.8 0 0; 0.4 0.8 0; 0 0.8 0.8; 0.4 0 0.8]; % for coil 4

% old
% if (nargin >= 5) && (~isempty(figHandle))
%     if ~figHandle
%         fig = myFigure;
%     else
%         fig = figHandle;
%     end
% else
%     fig = myFigure;
% end
if (nargin >= 5) && (~isempty(figHandle))
    fig = figHandle;
else
    fig = myFigure;
end


% if axisOff
%     axis off;
% end

figure(fig);
set(fig, 'color', 'white');


if normalLabels
    % save handles
    plotHandles = zeros(nClasses,1);
    for i=1:nClasses
        if dim == 1
            tmp = plot(zeros(size(X(labels==uniqueLabels(i)),1),1), ...
                X(labels==uniqueLabels(i)), ...
                symbs{i}, 'MarkerSize', markerSize, 'MarkerFaceColor', squeeze(cm(i,1,:)), ...
                'Color', squeeze(cm(i,1,:)), 'MarkerEdgeColor', squeeze(cm(i,3,:)));
        end
        if dim == 2
             tmp = plot(X(labels==uniqueLabels(i),1), ...
                X(labels==uniqueLabels(i),2), ...
                symbs{i}, 'MarkerSize', markerSize, 'MarkerFaceColor', squeeze(cm(i,1,:)), ...
                'Color', squeeze(cm(i,1,:)), 'MarkerEdgeColor', squeeze(cm(i,3,:)));
        elseif dim == 3
            tmp = plot3(X(labels==uniqueLabels(i),1), ... 
                X(labels==uniqueLabels(i),2), X(labels==uniqueLabels(i),3), ...
                symbs{i}, 'MarkerSize', markerSize, 'MarkerFaceColor', squeeze(cm(i,1,:)), ...
                'Color', squeeze(cm(i,1,:)), 'MarkerEdgeColor', squeeze(cm(i,3,:)));
        end
        if ~isempty(tmp)
            plotHandles(i) = tmp;
        end
        hold on; 
    end
else
    if dim == 2
        plotHandles = scatter(X(:,1), X(:,2), markerSize^2, labels, symbs, 'filled');
    elseif dim == 3
        plotHandles = scatter3(X(:,1), X(:,2), X(:,3), markerSize^2, labels, symbs, 'filled');
    end
    colormap(cm);
    hold on;
    
    % old variant
%     for i=1:numel(uniqueLabels)
%         if dim == 2
%             tmp = plot(X(labels==uniqueLabels(i),1), ...
%                 X(labels==uniqueLabels(i),2), ...
%                 symbs{i}, 'MarkerSize', markerSize, 'MarkerFaceColor', cm(uniqueLabels(i),:), ...
%                 'Color', cm(uniqueLabels(i),:));
%         elseif dim == 3
%             tmp = plot3(X(labels==uniqueLabels(i),1), ...
%                 X(labels==uniqueLabels(i),2), X(labels==uniqueLabels(i),3), ...
%                 symbs{i}, 'MarkerSize', markerSize, 'MarkerFaceColor', cm(uniqueLabels(i),:), ...
%                 'Color', cm(uniqueLabels(i),:));
%         end
%         if ~isempty(tmp)
%             plotHandles(i) = tmp;
%         end
%         hold on; 
%     end
end

if plotAxis
    grid on;
    xlabel('Dim 1',  'FontSize', fontSizeFigs, 'FontWeight', 'bold'); % 22
    ylabel('Dim 2',  'FontSize', fontSizeFigs, 'FontWeight', 'bold');
    if dim == 3
        zlabel('Dim 3',  'FontSize', fontSizeFigs, 'FontWeight', 'bold');
    end
end


%range = [-60 60 -70 50];
%axis(range);
% or as an alternative argument for lengend: 
%  legend(h, strcat('contig_',strtrim(cellstr(num2str(unique(Labels'))))), 'Location', 'BestOutside', 'Interpreter', 'none')
%  where h is a series of handles returned by plot: h(i) = plot(...).
%h_legend = legend('1', '2', '3', '4', '5', '6', '7', '8', '9', '0'); set(h_legend,'FontSize',16);
%h_legend = legend('ACA', 'ACC');
%h_legend = legend('intron', 'exon');
%h_legend = legend('C1', 'C2', 'C3');
%h_legend = legend(plotHandles, {'C1', 'C2', 'C3'});
% pos = [0.8818 0.0319 0.0840 0.5177];
%pos = [0.0316 0.0788 0.0970 0.6708];
%set(h_legend, 'Position', pos);
%range = axis;
%axis(range);

% legend handle bekommen
% tmp = findobj(gcf,'Type','axes','Tag','legend')


% determine the string for the labeling automatically
legendStr = cell(nClasses,1);
for i=1:nClasses
   legendStr{i} = ['C ' int2str(uniqueLabels(i))];
end
%legendStr = {'ACA', 'ACC'};
%legendStr = {'Bach', 'Beethoven', 'Haydn', 'Mozart', 'Scarlatti'};
%legendStr = {'bubble', 'insertion'};

if normalLabels
    h_legend = legend(plotHandles, legendStr);
    set(h_legend,'FontSize',fontSizeFigs); set(h_legend,'location','best');
end

if ~plotAxis
    if dim==2
        set(gca,'XTickLabel','','yTickLabel','','xtick',-100:10:-110,'ytick',-100:10:-110);
    elseif dim==3
        set(gca,'XTickLabel','','yTickLabel','', 'zTickLabel', '','xtick',-100:10:-110,'ytick',-100:10:-110,'ztick',-100:10:-110);
    end
end

set(gca, 'FontSize', fontSizeFigs, 'FontWeight', 'bold' );
box on;


if ~normalLabels
    colorbar;
    %cbar_handle = findobj(fig,'tag','Colorbar');
    %set(cbar_handle, 'YTick', []);
    
    % modify the caxis such that the highest value doesn't get white
    crange    = caxis;
    diffCr    = crange(2) - crange(1);
    tol       = 0.1;
    crange(1) = crange(1) - tol*diffCr;
    crange(2) = crange(2) + tol*diffCr;
    caxis(crange);
end

% set view of an old figure
% [AL, EL] = view; view(AL, ELL);
% currAx = axis; axis(currAx);


