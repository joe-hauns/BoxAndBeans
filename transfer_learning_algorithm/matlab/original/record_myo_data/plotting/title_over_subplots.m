function [ax,h]=title_over_subplots(text)

%

%Centers a title over a group of subplots.

%Returns a handle to the title and the handle to an axis.

% [ax,h]=subtitle(text)

%           returns handles to both the axis and the title.

% ax=subtitle(text)

%           returns a handle to the axis only.

fontSizeFigs = 24;

ax=axes('Units','Normal','Position',[.075 .075 .85 .85],'Visible','off');

set(get(ax,'Title'),'Visible','on')

title(text, 'FontSize', fontSizeFigs);

if (nargout < 2)

    return

end

h=get(ax,'Title');
