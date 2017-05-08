classdef spiderView2 < handle
  % polar plot for 1 or more polar circles for real-time use
  % by janne.hahne@bccn.uni-goettingen.de
  % Nov. 2015
  properties
    h_figure;
    h_axes;
    h_plot;
    
    blockDur;

    plot_scale
    data2r;
    angles;
    cosAngle;
    sinAngle;
    nAngles;
    nCurves;
    
    surf_h;
    
    enableAutoScale;
  end
  
  methods
    function obj = spiderView2(data2r,nAngles,nCurves,scale,angleStr,curveStr,angleOffset)
      obj.h_figure = figure(875);
      clf;
      
      obj.blockDur = 0.1; % in s
      %obj.plot_scale = [0 1];
      
      if (nargin < 5)
        for angle_idx = 1:nAngles
          angleStr{angle_idx} = ['Ch ' num2str(angle_idx)];
        end
      end
      
      if (nargin < 6)
        for curve_idx = 1:nCurves
          curveStr{curve_idx} = num2str(curve_idx);
        end
      end
      
      if (nargin < 7)
        angleOffset = 0;
      end
      
      obj.nAngles = nAngles;
      obj.nCurves = nCurves;
      
      nPoints = size(data2r,1);
      
      obj.data2r = data2r;
      obj.angles = 3/2 * pi - angleOffset*pi/180 + linspace(0,2*pi - 2*pi/nAngles, nAngles)';    
            
      obj.cosAngle = repmat(cos(obj.angles),1,nCurves);
      obj.sinAngle = repmat(sin(obj.angles),1,nCurves);
      
      if nargin > 1
        obj.plot_scale = scale;
        obj.enableAutoScale = false;
      else
        obj.enableAutoScale = true;
      end
                 
     % obj.h_figure = figure('doublebuffer','on');
      set(gca,'drawmode','fast');


      
      obj.h_plot = plot(zeros(2,nCurves),'linewidth',2);
     
      hold on;
      
      plot([-1 0;1 0],[0 -1; 0 1],'k');

      
      textRadius = 0.85;
      for angle_idx = 1:obj.nAngles
        text(textRadius * obj.cosAngle(angle_idx),textRadius * obj.sinAngle(angle_idx),angleStr{angle_idx});
      end      
      
      if nCurves > 1
          legend(curveStr);
      end
      
      xlim([-1 1]);
      ylim([-1 1]);    
      obj.h_axes = gca;
        

    end
    
    
    function update(obj,data)
          
      if obj.enableAutoScale
       data = data/max(data);
      else
       data = data * obj.plot_scale;
      end    
      
      r = reshape(obj.data2r * data,obj.nAngles,obj.nCurves);
      
      x = r .* obj.cosAngle;
      y = r .* obj.sinAngle;
      
      for curve_idx = 1:obj.nCurves
        set(obj.h_plot(curve_idx),'xdata',[x(:,curve_idx) ;x(1,curve_idx)],'ydata',[y(:,curve_idx) ;y(1,curve_idx)]);% update plot
      end
        
    end
  
  end

  
end
