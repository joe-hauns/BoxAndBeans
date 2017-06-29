function [colors, names] = tango_color_scheme( )

% see http://sobac.com/sobac/tangocolors.htm

colors = zeros(9, 3, 3);
names = cell(9);

c = 1;

names{c} = 'scarletred';
colors(c, 1, :) = [239,41,41];
colors(c, 2, :) = [204,0,0];
colors(c, 3, :) = [164,0,0];
c = c+1;

names{c} = 'skyblue';
colors(c, 1, :) = [114,159,207];
colors(c, 2, :) = [52,101,164];
colors(c, 3, :) = [32,74,135];
c = c+1;

names{c} = 'chameleon';
colors(c, 1, :) = [138,226,52];
colors(c, 2, :) = [115,210,22];
colors(c, 3, :) = [78,154,6];
c = c+1;

names{c} = 'orange';
colors(c, 1, :) = [252,175,62];
colors(c, 2, :) = [245,121,0];
colors(c, 3, :) = [206,92,0];
c = c+1;

names{c} = 'plum';
colors(c, 1, :) = [173,127,168];
colors(c, 2, :) = [117,80,123];
colors(c, 3, :) = [92,53,102];
c = c+1;

names{c} = 'butter';
colors(c, 1, :) = [252,233,79];
colors(c, 2, :) = [237,212,0];
colors(c, 3, :) = [196,160,0];
c = c+1;

names{c} = 'aluminium';
colors(c, 1, :) = [238,238,236];
colors(c, 2, :) = [211,215,207];
colors(c, 3, :) = [186,189,182];
c = c+1;

names{c} = 'chocolate';
colors(c, 1, :) = [233,185,110];
colors(c, 2, :) = [193,125,17];
colors(c, 3, :) = [143,89,2];
c = c+1;

names{c} = 'slate';
colors(c, 1, :) = [136,138,133];
colors(c, 2, :) = [85,87,83];
colors(c, 3, :) = [46,52,54];
c = c+1;

colors = colors ./ 256;

end

