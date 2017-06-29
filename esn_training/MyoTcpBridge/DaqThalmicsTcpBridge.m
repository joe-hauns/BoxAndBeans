classdef DaqThalmicsTcpBridge < handle
  % Interface for one or two MYOs via MYO-TCP-BRIDG
  %
  % by janne.hahne@bccn.uni-goettingen.de Nov. 2015
  
  properties
    fs;
    channels;
    running;
    
    blockSize;
    blockDuration
    dataType;
    scaleInt2Volt;
    
    con;
    
    autoStart;
    isEventBased;
    port;
  end
  
  methods
    function obj = DaqThalmicsTcpBridge(nmyos,autoStart,port)
      % consturctur: initializes everything and establishes TCP/IP connection
      % to MyoBilateralConnector.
      % Start Myo Connect before, connect to both MYOs and run 
      % MyoBilateralConnector.exe before!

      
      if nargin < 3
        obj.port = 3333;
        disp('using port 3333 for connection with myoConnect');
      else
         obj.port = port;
      end
      
      obj.autoStart = autoStart; % automatically start connector proggram?
      
      obj.isEventBased = false;
       
      obj.fs = 200;
      
      obj.dataType = 'int8';

      obj.blockSize = 8; % standard block size (number of samples received in each getData call)
      obj.blockDuration = obj.blockSize/obj.fs;
 
      obj.running = false;
      
      obj.scaleInt2Volt = 10* 10^(-6);
      
      if obj.autoStart
        % search if an old connector is allready running:
        %[error,message] = dos('TASKLIST /FI "imagename eq MyoConnectorUniversal.exe" /svc');
        
        % kill any potentially running old connector:
        [error_id,message] = dos('taskkill /F /IM MyoTcpBridge.exe');
        if ~error_id % no error -> there was an old connector running
          disp('Found and killed an old MyoConnector');
        end
        
        % start new myoConnector:
        %disp('Trying to start new MyoConnector, check the cmd window for details');  
        %[error_id,message] = dos(['..\daq\Thalmics\MyoTcpBridge.exe ' num2str(obj.port) ' &']);   
        fullfilePath=which('MyoTcpBridge.exe');
        system([fullfilePath ' ' num2str(obj.port)  ' &']);
      end
      
      pause(0.3);
      
      obj.con=pnet('tcpconnect','localhost',obj.port);

      if nmyos == 1
        obj.channels = 1:8;
        pnet(obj.con,'write', '1myo' );
      pause(0.1);
      elseif nmyos == 2
        obj.channels = 1:16;
        pnet(obj.con,'write', '2myo' );
      end
      
      
    end
    
    function delete(obj)
      try
      if obj.running
        obj.stopDaq();
      end

      if obj.autoStart          
        % tell MyoTcpBridge to close:
        disp('close MyoTcpBridge.exe...');
        pnet(obj.con,'write', 'close' );
        pause(0.1);
      end
      
      catch
        disp('error while closing client');
      end
    end
    
    function startDaq(obj)
      % starts DAQ, you should then regularly collect the data with
      % getData() and run stopDaq afterwards!
      
      % clear TCP/IP-buffer:
      pnet(obj.con,'setreadtimeout',0);
      pnet(obj.con,'read');% emplty buffer
      
      % tell server to start:
      pnet(obj.con,'write', 'startmyo' );
      pause(0.1);
      
      % empty TCP buffer
      newdata = nan;
      while ~(isempty(newdata))
        newdata=pnet(obj.con,'read',[length(obj.channels) obj.blockSize] ,'int8');
      end
          
      obj.running = true;
    end
    
    function stopDaq(obj) 
      % stops the DAQ
      
      % tell MyoTcpBridge to stop:
      pnet(obj.con,'write', 'stopmyo' );
      pnet(obj.con,'setreadtimeout',inf);
      
      obj.running = false;
    end
    
    function data = getData(obj)
      % get all data from MyoTcpBridge since last call
      
      if obj.running;
        data = [];
        newdata = nan;
        while obj.running && (isempty(data) || ~isempty(newdata)) % collect all TCP-packets that are in buffer
          newdata=pnet(obj.con,'read',[length(obj.channels) obj.blockSize] ,'int8');
          data = [data newdata];
        end
				newdata
      else
        data = [];
        warning('daq not started!')
      end
     
    end
    
  end 
end

