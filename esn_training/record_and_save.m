function record_and_save(name, left_or_right, date)

	addpath('storage_utils')

	% usr input for data storing
	run = input_uint('How many runs did you already store today?');

	dir_name = data_dir(name, left_or_right, date);

	if 0 == exist(dir_name, 'dir')
		mkdir(dir_name);
	else
		assert(7 == exist(dir_name, 'dir'));
	end
	
	while 1
		[X_logVar, targets] = record_myo_data;

		%close all;
		if input_option('Do you want to store the recording?', 'y', 'n') == 'y' 
			run = run + 1;
			save(data_file(name, left_or_right, date, run), 'X_logVar', 'targets');
		end

		if input_option('One more run?', 'y', 'n') == 'n';
			break;
		end

	end

	
end

function [output] = input_option(question_string, option1, option2)
	fprintf('%s (%s/%s)', question_string, option1, option2)
	while ~exist('output', 'var')
		inp = input('', 's');
		if all(inp == option1) || all(inp == option2)
			output = inp;
		end
	end
end

function [out] = input_uint(question_string)
	fprintf(question_string);
	while ~exist('out', 'var')
		inp = input('');
		if isnumeric(inp) && int8(inp) >= 0
			out = int8(inp);
		else
			fprintf('Please enter a number >= 0: ')
		end
	end
end
