classdef combFilter < handle
  %
  % Comb filter for real-time use by
  % janne.hahne@bccn.uni-gettingen.de
  % Nov. 2015

  properties
    B_comb;
    A_comb;
    z_comb;
  end
  
  methods 
    function obj = combFilter(fs,fComb)
      % fs: sample rate
      % fComb: filter-freq, e.g. 50 or 60
      
      % create filter:
      delay = round((fs / fComb)) - 1;
      obj.B_comb = [1 zeros(1,delay) -1]; % num. coef. of comb filter
      obj.A_comb = [1 zeros(1,delay) -0.75]; % den. coef.. of comb filter
    end
    
    function reset(obj)
      % resets filters
     
      obj.z_comb = [];
    end
    
    function data = filter(obj,data)
    % filters one block of data
    % data is of size <N_channels x N_Samples>
      
      
        [data, obj.z_comb] = filter(obj.B_comb, obj.A_comb , data, obj.z_comb, 2);

      
    end
  end
  
end

