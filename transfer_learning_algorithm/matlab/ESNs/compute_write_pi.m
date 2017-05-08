% create pi to a certain precision and write it to a file

% compute Pi for a reservoir of size 1000 with 8 inputs
num_digits = 8*1000;
p = chud_pi(num_digits);

% convert the sym number to a string
piStr    = char(p);
% represent each digit as an entry in a vector
piVec    = zeros(numel(piStr)-1,1);
piVec(1) = 3;
for i = 3:numel(piStr)
    piVec(i-1) = str2double(piStr(i));
end

% write it to a file
savejson('pi8000', piVec,'pi_8000.json');

% --- don't use the csv format for strings. they do not load
% csvwrite(sprintf('pi_%i.csv', num_digits), piStr);