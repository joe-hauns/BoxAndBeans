function [file] = esn_file(name, left_or_right, date)
	dir = esn_dir(name, left_or_right, date);
	file = [dir filesep 'ESN_' datestr(now, 'yyyy-mm-dd_HH:MM:SS') '.json'];
end
