function [X_meanAbs, X_logVar, prev_data, new_data] = read_data_calc_feats(myDaqObj, fiterObj, prev_data)
% This function uses the MyoTcpBridge to read data from a connected Myo and
% computes two features: the mean absolute value and the log of the
% variance. It uses a fixed time window of 24, where 16 new samples and
% fetched from the device and 8 old ones are used.

performTestRun = 0;

blocksize = 8;
new_data  = zeros(8, 2*blocksize);

% retrieve data
if performTestRun
    pause(0.01)
    tmp = 4*rand(blocksize,8);
else
    tmp = myDaqObj.getData();
end
% take only the last blocksize samples
new_data(:,1:blocksize)     = tmp(:,end-blocksize+1:end);
if performTestRun
    tmp = 4*rand(blocksize,8);
else
    tmp = myDaqObj.getData();
end
new_data(:,blocksize+1:end) = tmp(:,end-blocksize+1:end);

% compute a feature per channel
if ~isempty(prev_data)
    if performTestRun
        tmp_X = [prev_data new_data];
    else
        tmp_X = fiterObj.filter(double([prev_data new_data]));
    end
    % the mean absolute value
    X_meanAbs = mean(abs(tmp_X),2)';
    % the log of the variance
    X_logVar  = log(var(tmp_X'));
else
    X_meanAbs = nan;
    X_logVar  = nan;
end

prev_data = new_data(:, end-blocksize+1:end);
