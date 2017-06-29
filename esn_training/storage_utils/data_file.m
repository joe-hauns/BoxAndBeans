function [file] = data_file(name, left_or_right, date, run)

	assert(isinteger(run));

	dir = data_dir(name, left_or_right, date);
	file = [dir filesep sprintf('%02d', run) '.mat'];
end
