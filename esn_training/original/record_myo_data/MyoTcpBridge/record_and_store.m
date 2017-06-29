function record_and_save
	% usr input for data storing
	name = input('Name: ', 's'); 
	left_or_right = input_option('Left or right hand?', 'left', 'right');
	date = datestr(now, 'yyyy-mm-dd');
	run = input_uint('How many runs did you already store today?');

	% usr input for data storing
	dir_name = sprintf('../../myo_train_data_jo/myodata_%s_%s_%s', name, left_or_right, date);

	if 0 == exist(dir_name, 'dir')
		mkdir(dir_name);
	else
		assert(7 == exist(dir_name, 'dir'));
	end
	
	while 1
		record_data_from_myo;

		%close all;
		if input_option('Do you want to store the recording?', 'y', 'n') == 'y' 
			run = run + 1;
			file_name = sprintf('%s/myodata_%s_%s_%s_%d_src.mat', dir_name, name, left_or_right, date, run);
			save(file_name);
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
