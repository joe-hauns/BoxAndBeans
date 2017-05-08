function picture_as_pdf( filename, width, height )
% Stores the current figure as pdf and adjusts the width and height
% beforehand as specified in the input arguments (in centimeters)

if(nargin < 2)
    width = 15;
end
if(nargin < 3)
    height = 15;
end

set(gcf, 'PaperUnit', 'centimeters');
set(gcf, 'PaperSize', [width, height]);
set(gcf, 'PaperPosition', [0, 0, width, height]);

print('-dpdf', filename);

end

