function main(command, name, left_or_right)

	if ischar(command) && ischar(name) && (strcmp(left_or_right, 'left') || strcmp(left_or_right, 'right')) 

		date =  datestr(now, 'yyyy-mm-dd');

		if strcmp(command, 'record')
			record_and_save(name, left_or_right, date);
		elseif strcmp(command, 'train')
			runs = int16(str2num(input('How many runs to load?','s')));
			load_and_train(name, left_or_right, date, runs);
			
		else 
			fprintf('usage: main <command> <name> <left_or_right>\n');
			fprintf('possible commands: record, train, clear\n');
		end
	else
		
		fprintf('usage: main <command> <name> <left_or_right>\n');
		if ~ischar(command) 
				fprintf('<command> must be a string. possible commands: record, train, clear\n');
		end
		if ~ischar(name) 
				fprintf('<name> must be a string\n');
		end
		if ~(strcmp(left_or_right, 'left') || strcmp(left_or_right, 'right')) 
				fprintf('<left_or_right> must be eighter ''left'' or ''right''\n');
		end

	end
end


