function [acc,classwiseAcc] = eval_contin_fun_pred(targets, preds, woSteps, showResults, colors, col_dofs, model)
% This function receives a target signal and a prediction of a model.
% It excepts the targets to be scaled from -1 to 1, with a three class
% encoding consisting of classes -1, 0 and +1. The function removes all the
% values in between this 

if nargin < 4
    showResults = 0;
end
if nargin < 7
    model = '';
end

if ~iscell(targets)
    targets = {targets};
    preds   = {preds};
end

% compe an accuracy for each class in each Dof
dofs    = size(targets{1},2);
% there are 3 classes per degree of freedom: -1,0,1
classes = [-1 0 1];

numSeq        = numel(targets);
accs          = zeros(numSeq, dofs);
classwiseAccs = zeros(numel(classes), dofs, numSeq);
for seq = 1:numSeq
    % remove the first woSteps instances
    predsS      = preds{seq}(woSteps:end,:);
    targetsS    = targets{seq}(woSteps:end,:);
    % remove the intermediate segments
    addr_vec    = (abs(targetsS) <= 0.001) + (abs(targetsS) >= 0.99);
    addr_vec    = logical(min(addr_vec, [], 2));
    preds_cut   = round(predsS(addr_vec, :));
    targets_cut = round(targetsS(addr_vec, :));

    % compute the overall accuracy
    accs(seq,:) = sum(preds_cut == targets_cut)/size(preds_cut,1);

    for dof = 1:dofs
        for cl = 1:numel(classes)
            curr_class_idx = classes(cl) == targets_cut(:, dof);
            curr_pred      = preds_cut(curr_class_idx, dof);
            % compute the accuracy of class cl in degree of freedom dof
            classwiseAccs(cl, dof, seq) = sum(curr_pred == classes(cl))/sum(curr_class_idx);
        end
    end
end
acc          = sum(accs,1)/numSeq;
classwiseAcc = sum(classwiseAccs,3)/numSeq;


% print the accuracy?
if showResults > 0
    if dofs == 1
        disp(['=========' model '=Performance=========']);
        fprintf('               DoF1     \n');
        fprintf(' overall acc: %6.2f%%   \n', 100*acc(1));
        disp(' -------------------------------');
        fprintf(' cl ''-1'' acc: %6.2f%%    \n', 100*classwiseAcc(1,1));
        fprintf(' cl  ''0'' acc: %6.2f%%    \n', 100*classwiseAcc(2,1));
        fprintf(' cl ''+1'' acc: %6.2f%%    \n', 100*classwiseAcc(3,1));
        disp('=================================');
    elseif dofs == 2
        disp(['=========' model '=Performance=========']);
        fprintf('               DoF1      DoF2\n');
        fprintf(' overall acc: %6.2f%%    %6.2f%%\n', 100*acc(1), 100*acc(2));
        disp(' -------------------------------');
        fprintf(' cl ''-1'' acc: %6.2f%%    %6.2f%%\n', 100*classwiseAcc(1,1), 100*classwiseAcc(1,2));
        fprintf(' cl  ''0'' acc: %6.2f%%    %6.2f%%\n', 100*classwiseAcc(2,1), 100*classwiseAcc(2,2));
        fprintf(' cl ''+1'' acc: %6.2f%%    %6.2f%%\n', 100*classwiseAcc(3,1), 100*classwiseAcc(3,2));
        disp('=================================');
    elseif dofs == 3
        fprintf('The accuracy amounts to %5.2f%% %5.2f%% %5.2f%% \n', ...
            100*acc(1), 100*acc(2), 100*acc(3));
        % TODO: add the detailed accuracies
    end
end

% do visualizations?
if showResults > 1
    myFigure; hold on;
    nSamples = size(targets_cut,1);
    hTar  = zeros(dofs,1);
    hPred = zeros(dofs,1);
    if showResults == 2
        prettyShift = [0.005 -0.005];
    elseif showResults == 3
        prettyShift = [0 0];
    end

    % plot the target curve and the predictions
    for dof=1:dofs
        if showResults == 3
            subplot(dofs,1,dof); hold on;
        end

        hTar(dof)  = plot(1:nSamples, targets_cut(:, dof)+prettyShift(dof), '-', 'Color', squeeze(colors(col_dofs(dof), 1, :)));
        hPred(dof) = plot(1:nSamples, preds_cut(:, dof), 'o', 'Color', squeeze(colors(col_dofs(dof), 3, :)));
        
        currAx = axis;
        axis([0 currAx(2) -1.1 1.1])
    end

    % add a legend and the accuracy
    if showResults == 2
        % use a brief accuracy description
        legend([hTar; hPred], {'DoF1 gt', 'DoF2 gt', 'DoF1 pred', 'DoF2 pred'});
        title(sprintf('%s Accuracy: DoF1: %5.2f%%, DoF2: %5.2f%%', model, 100*acc(1), 100*acc(2)));
    elseif showResults == 3
        % provide a detailed accuracy description
        for dof=1:dofs
            subplot(dofs,1,dof);
            currDof = sprintf('DoF%i', dof);
            legend([hTar(dof); hPred(dof)], {[currDof ' gt'], [currDof ' pred']});

            title(sprintf('%s accs in DoF%i: %2.0f%%, %2.0f%%, %2.0f%%', ...
                model, dof, 100*classwiseAcc(1,dof), ...
                100*classwiseAcc(2,dof), 100*classwiseAcc(3,dof)));
        end

    end

end