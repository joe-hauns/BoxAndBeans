function fig = myFigure(varargin)

wide = 0;
scaling_wide = 1;
if nargin > 0
    type = varargin{1};
    if strcmpi(type, 'wide')
        wide = 1;
        scaling_wide = 1.5;
    end
end


scrsz = get(0,'ScreenSize');
bottomPos = round(scrsz(4)/(2.8)); % old: /(2.77));
heightFig = round(scrsz(4)/1.6); % old 1.8
widthFig  = round(1.5*heightFig * scaling_wide);

fig = figure('OuterPosition',[2 bottomPos widthFig heightFig]); 
set(fig, 'color', 'white');
