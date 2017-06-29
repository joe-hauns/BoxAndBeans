function [dir] = data_dir(name, left_or_right, date)

	assert(strcmp(class(name) , 'char'));
	assert(strcmp(class(date) , 'char'));
	assert(strcmp(left_or_right , 'left') || strcmp(left_or_right , 'right'));

	dir = ['trained_esns' filesep name filesep left_or_right filesep date ]; 
end
