function main(command)

	name = 'jo';
	left_or_right = 'right';
	date =  datestr(now, 'yyyy-mm-dd');

	if strcmp(command, 'record')
		record_and_save(name, left_or_right, date);
	elseif strcmp(command, 'train')
		runs = int16(str2num(input('How many runs to load?','s')));
		load_and_train(name, left_or_right, date, runs);
		
	else 
		fprintf('possible commands: record, train, clear');
	end
end


