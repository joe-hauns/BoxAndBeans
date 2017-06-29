% EDITED
function load_and_train(name, left_or_right, date, runs)

	addpath('storage_utils');
	addpath('plotting');
	addpath('jsonlab');
	addpath('MyoTcpBridge');
	addpath('ESNs');
	addpath('myo');

	% skip the first wpSteps samples from the evaluation
	woSteps = 30;
	model   = 'ESN';
	if ~isequal(lower(model), 'esn')
			woSteps = 1;
	end
	cutSequences = 1;
	nMovesPerSeq = 8;

	% load src data
	[X_src, Y_src, fold_idxs_src] = read_cut_emg_data(name, left_or_right, date, runs, model, woSteps, cutSequences, nMovesPerSeq);
% END EDITED

	dimIn  = size(X_src,2);
	dimOut = size(Y_src,2);

	% set constants for colors
	colors        = tango_color_scheme();
	col_dofs      = [1 2];
	col_dofIntens = [1 1];


	%% compute the leave-one-run-out accuracy on the source data
	useGlobAcc     = 1;
	num_dofs       = size(Y_src, 2);
	num_classes    = 3;
	classes        = [-1 0 1];
	linearESN      = 0;
	N              = 300;  % 300
	estimateParams = 0;

	useReLU = 0;
	% in case crj is used - just for checking
	crjPars = [0.4 0.54 0.54 20];
	if useReLU
			psdRes     = 1;
			universRes = 0;
	else
			psdRes     = 0;
			universRes = 1;
	end

	% fixed seed set for reproducible results
	rng(1042);
	seedSet = randi(10000,5,1);


	if useGlobAcc
			err.train   = inf(numel(fold_idxs_src), num_dofs);
			err.test    = inf(numel(fold_idxs_src), num_dofs);
	else
			err.train   = inf(numel(fold_idxs_src), num_dofs, num_classes);
			err.test    = inf(numel(fold_idxs_src), num_dofs, num_classes);
	end



	tic;

if estimateParams
    % define candidates for the parameters
    reg_cands     = 10.^(-3:2); %10.^(-3:1);
    dt_cands      = 0.4:0.1:0.6;
    % seedSet
    specRad_cands = 0.9:0.02:1;
    
    % generate all random parameters
    n_paramSearch = 100;
    curr_params   = [randi(numel(reg_cands),1,n_paramSearch); ...
                        randi(numel(dt_cands),1,n_paramSearch); ...
                        randi(numel(seedSet),1,n_paramSearch); ...
                        randi(numel(specRad_cands),1,n_paramSearch)];
    cross_val_aver_acc = zeros(n_paramSearch, numel(fold_idxs_src));


    for r=1:numel(fold_idxs_src)
        fprintf('\n\nFold %d of %d\n\n', r, numel(fold_idxs_src));

        % generate indices for training and testing
        Idx_train = true(size(X_src, 1), 1);
        Idx_train(fold_idxs_src{r}, :) = false;

        % split the training set into a train and validation set
        X_src_tr_whole = X_src(Idx_train, :);
        Y_src_tr_whole = Y_src(Idx_train, :);
        num_val = round(0.3 * size(X_src_tr_whole,1));
        randPos = randi(size(X_src_tr_whole,1) - num_val,1,1);
        idx_val = randPos+1:randPos+num_val;
        idx_tr  = [1:randPos, randPos+num_val+1:size(X_src_tr_whole,1)];

        % define the training and the validation set
        X_src_tr  = X_src_tr_whole(idx_tr,:);
        X_src_val = X_src_tr_whole(idx_val,:);
        Y_src_tr  = Y_src_tr_whole(idx_tr,:);
        Y_src_val = Y_src_tr_whole(idx_val,:);


        % train a model and compute the prediction
        % perform a model selection step
        % try the possible parameters
        for i = 1:n_paramSearch
            rng(seedSet(curr_params(3,i)));

            lspecs = {'class','ESN'; 'hidDim',N; 'reg',reg_cands(curr_params(1,i)); ...
                'woSteps',woSteps; 'inpDim',dimIn; 'outDim',dimOut; ...
                'dt', dt_cands(curr_params(2,i)); 'specRad', ...
                specRad_cands(curr_params(4,i)); 'universRes', universRes; ...
                'linearESN', linearESN; 'useReLU', useReLU; 'psdRes', psdRes; ...
                'universResPars', crjPars};
            esn = ESN(dimIn, dimOut, lspecs);
            esn.init();
            esn.train(X_src_tr, Y_src_tr);

            % comppute the prediction on the validation set
            preds_val  = esn.apply(X_src_val);
            [acc, classwiseAcc] = eval_contin_fun_pred(Y_src_val, preds_val, woSteps, 0);
            if useGlobAcc
                cross_val_aver_acc(i,r) = mean(acc);
            else
                cross_val_aver_acc(i,r) = mean(classwiseAcc(:));
            end
        end
    end

    % select the best paremters
    [~, max_idx] = max(mean(cross_val_aver_acc,2));
    bestParams = [reg_cands(curr_params(1,max_idx)), ...
        dt_cands(curr_params(2,max_idx)), ...
        seedSet(curr_params(3,max_idx)), ...
        specRad_cands(curr_params(4,max_idx))];
    bestParams
end
toc;


if estimateParams
    lspecs = {'class','ESN'; 'hidDim',N; 'reg',bestParams(1); ...
        'woSteps',woSteps; 'inpDim',dimIn; 'outDim',dimOut; ...
        'dt', bestParams(2); 'specRad', bestParams(4); 'universRes', universRes; ...
        'linearESN', linearESN; 'useReLU', useReLU; 'psdRes', psdRes; ...
        'universResPars', crjPars};
else
    reg     = 2.5;
    dtCons  = 0.45;
    specRad = 0.9;
    lspecs = {'class','ESN'; 'hidDim',N; 'reg',reg; ...
        'woSteps',woSteps; 'inpDim',dimIn; 'outDim',dimOut; ...
        'dt', dtCons; 'specRad', specRad; 'universRes', universRes; ...
        'linearESN', linearESN; 'useReLU', useReLU; 'psdRes', psdRes; ...
        'universResPars', crjPars}; 
end


% compute the test error
for r=1:numel(fold_idxs_src)
    if estimateParams
        rng(bestParams(3));
    else
        rng(seedSet(1));
    end
    % generate indices for training and testing
    Idx_train = true(size(X_src, 1), 1);
    Idx_train(fold_idxs_src{r}, :) = false;
    
    esn = ESN(dimIn, dimOut, lspecs);
    esn.init();
    esn.train(X_src(Idx_train,:), Y_src(Idx_train,:));

    preds_train = esn.apply(X_src(Idx_train,:));
    preds_test  = esn.apply(X_src(Idx_train==false, :));

    % evaluate the train error
    [acc, classwiseAcc] = eval_contin_fun_pred(Y_src(Idx_train, :), preds_train, woSteps, 1, colors, col_dofs,model);
    if useGlobAcc
        err.train(r, :)    = 1-acc;
    else
        err.train(r, :, :) = 1-classwiseAcc';
    end
    
    % evaluate the test error
    [acc, classwiseAcc] = eval_contin_fun_pred(Y_src(Idx_train==false, :), preds_test, woSteps, 1, colors, col_dofs,model);
    if useGlobAcc
        err.test(r, :)    = 1-acc;
    else
        err.test(r, :, :) = 1-classwiseAcc';
    end
end


clear X_src_tr X_src_tr_whole Y_src_tr Y_src_tr_whole X_src_val Y_src_val

if useGlobAcc
    % compute the average performance
    averErr = mean(err.test);
    stdDev  = std(err.test);
else
    % compute the average performance
    averErr = zeros(2,3);
    for r=srcDataSets
        averErr = averErr + squeeze(err.test(r,:,:));
    end
    averErr = averErr/numel(fold_idxs_src);

    % compute also the standard deviation
    stdDev = zeros(2,3);
    for r=srcDataSets
        stdDev = stdDev + (squeeze(err.test(r,:,:)) - averErr).^2;
    end
    stdDev = stdDev/numel(fold_idxs_src);
    stdDev = sqrt(stdDev);
end
averErr
stdDev

%% retrain ESN on all data

esn = ESN(dimIn, dimOut, lspecs);
esn.init();
esn.train(X_src, Y_src);

%{ REMOVED
% store ESN

esn.export('current_output_weights.json', 1);
esn.export('current_reservoir.json', 2);

%} END REMOVED

% ADDED

	esn_dir_name = esn_dir(name, left_or_right, date);
	mkdir(esn_dir_name);

	fprintf('exported jsons to %s\n', esn_dir_name);
	time = datestr(now, 'yyyy-mm-dd_HH-MM-SS');
	esn.export([esn_dir_name filesep time '_out.json'], 1);
	esn.export([esn_dir_name filesep time '_reservoir.json'], 2);

end
% END ADDED
