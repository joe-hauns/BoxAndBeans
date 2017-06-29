% DEMO how to use the MATLAB interface for one or two MYOs
%
% 1) Install and start Myo Connect from Thalmics Labs, conncet one or two
% MYOs, sync MYO(s) to avoid sleep-mode
%
% 2) Run this m file, the communication to the MYOs should be initialised,
% the MYO(s) should vibrate shortly
%
% 3) To capture and vizualize data for a few seconds execute the code section after
% the first break (right Mouse click on this part of code -> Evaluate Current Section
% or simply place the curser in the code and press Ctrl + Enter)
%
% by janne.hahne@bccn.uni-goettingen.de



% Settings:

N_myos = 1; % number of MYOs to use (1 or 2)
f_powerline = 50; % choose 50 or 60 Hz depending on your region
port = 3334; % port for TCP-IP connection



% Init

% close potentially existing old instances of "myDaqObj"
if exist('myDaqObj')
  disp('closing old MyoTcpBridge');
  myDaqObj.delete();
end

% create a DaqThalmicsTcpBridge object, automaticly start MyoTcpBridge.exe
% and establish connection:
myDaqObj = DaqThalmicsTcpBridge(N_myos,true,port);

% create filter object aginst powerline noise: 
fiterObj = combFilter(myDaqObj.fs,f_powerline);

if N_myos == 1
  % create spider-plot object with 1 curve:
  mySpiderObj = spiderView2(eye(8),8,1,1*10^(-2));
  
elseif N_myos == 2; 
  % create spider-plot object with 2 curves:
  mySpiderObj = spiderView2(eye(16),8,2,1*10^(-2));
else
  error('Only 1 or 2 MYOs supported');
end
  

%pause(2);
%break

%% 
% This code section should be executed with ctrl + Enter after sucessfull execution of the initialization code above
% it vizualizes the amplitude of the EMG in real-time in a polar plot

myDaqObj.startDaq();


for i=1:100
    data = myDaqObj.getData(); % get data
    data = fiterObj.filter(double(data));
    %filter data:
    mav = mean(abs(data),2); % compute MAV features
    mySpiderObj.update(mav); % update spider-plot
end
%
myDaqObj.stopDaq()


break

%% close everything in a clean way
myDaqObj.delete();

