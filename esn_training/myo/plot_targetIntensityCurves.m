function curveData = plot_targetIntensityCurves(figH, range, titleStr)
% This function plots a intensity curve which should be followed by the
% participant using the Myo. 
%
% It requires:
%   figH         a figure handle or an empty matrix. in the first case the
%                curve is drawn, in the latter only computed and returned
%   range        the maximum value of the x axis, i.e. how many data poitns
%                are used
%   titleStr     a string specifying the current action that should be
%                performed

doPlot = 0;
baselevel = 0.1;

if ~isempty(figH)
    doPlot = 1;
    figure(figH);
    cla; hold off;
end

switch lower(titleStr)
    case 'no movement'
        curve = 'flat';
    case {'calibration: relax and hand close','prediction','calibration'}
        curve = 'no curve';
    otherwise
        curve = 'high-low';
end

curveData = zeros(range,1);

switch curve
    case 'flat'
        curveData = baselevel*ones(range,1);
    case 'high-low'
        targetHeight = 0.7;
        
        % define the range of the two sigmoids
        part1 = 1:round(0.5*range);
        part2 = round(0.5*range)+1:range;
        % compute the values of the sigmoid on those positions
        curveData(1:numel(part1))     = ((targetHeight-baselevel)/2) ...
            * (1+tanh(linspace(-4,5,numel(part1)))) + baselevel;
        curveData(numel(part1)+1:end) = ((targetHeight-baselevel)/2) ...
            * (1+tanh(linspace(5,-4, numel(part2)))) + baselevel;
    case 'no curve'
        % do not plot a curve
        curveData = [];
end


if doPlot
    % plot the computed curvevalues
    plot(1:numel(curveData), curveData, '-b');
    axis([1 range -0.1 1]);
    title(titleStr);
    hold on;
end


