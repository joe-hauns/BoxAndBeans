function act_curve = generate_myo_training_curve(postures, win_len, pause_len)
% This function generates the activation curve that is targeted by the
% participants while recording data. It concatinates activation curves for
% the postures given in 'postures' of length 'win_len' and inserts
% 'pause_len' number of samples between them.

numPosts = numel(postures);
act_curve = zeros(numPosts*(win_len + pause_len), 1);

% use a loop such that this also works if individual curves differ for
% different movements
for p=1:numPosts
    offset = (p-1)*(win_len + pause_len);
    % insert the curve for the current posture
    act_curve(offset+1:offset+win_len) = ...
        plot_targetIntensityCurves([], win_len, postures{p});
    
    % insert a pause of no movement
    act_curve(offset+ win_len + 1:offset + win_len + pause_len) = ...
        plot_targetIntensityCurves([], pause_len, 'no movement');
end
