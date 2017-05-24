using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ProsthesisControllers;
using System.Timers;

public class SimpleStateMachineController : MyoController {

	public bool vibrateOnStateChange = false;
	public int sampleTimeDifferenceInMilliseconds = 10;
	public int filterWindowLengthInMilliseconds = 150;

	private bool rotate = true;
	private GameTime gameTime;
	private Timer timer;

	public override string controllerName { get { return "State Machine (Simple)"; } }
	public override string defaultConfigFile { get{ return Path.Combine(Application.persistentDataPath, "simple_state_machine_config.json"); } }
	public override bool needsConfiguration { get{ return true; } }
	public override ConfigResult SetConfiguration (string configPath) { 
		try {
			var config = JsonUtility.FromJson<SimpleStateMachineControllerDto> (File.ReadAllText (configPath));
			if (config.electrode1 < 0 || 7 < config.electrode1)
				return ConfigResult.error("Electrode indizes must be within [0; 7]. Found: electrode1 = "+config.electrode1);
			if (config.electrode2 < 0 || 7 < config.electrode2)
				return ConfigResult.error("Electrode indizes must be within [0; 7]. Found: electrode2 = "+config.electrode2);
			if (config.threshold1 < 0 || 1 < config.threshold1)
				return ConfigResult.error("Thresholds must be within [0; 1]. Found: threshold1 = "+config.threshold1);
			if (config.threshold2 < 0 || 1 < config.threshold2)
				return ConfigResult.error("Thresholds must be within [0; 1]. Found: threshold2 = "+config.threshold2);
			this.config = config;
			return ConfigResult.OK; 
		} catch (System.Exception e) {
			return ConfigResult.error (e.Message);
		}
	}
	private SimpleStateMachineControllerDto config;

	private Filter filter1;
	private Filter filter2;

	private bool electrode1 { get { return filter1.get () >= config.threshold1; } }
	private bool electrode2 { get { return filter2.get () >= config.threshold2; } }
	private bool electrodeOpen { get { return config.electrode1_open ? electrode1 : electrode2; } }
	private bool electrodeClose { get { return config.electrode1_open ? electrode2 : electrode1; } }
	private bool electrodeClockWise { get { return config.electrode1_rotate_clockwise ? electrode1 : electrode2; } }
	private bool electrodeCounterClockWise { get { return config.electrode1_rotate_clockwise ? electrode2 : electrode1; } }
	private float lastElectrode1 = -1;
	private float lastElectrode2 = -2;
	private bool cocontracting = false;

	protected override void _Update () { 

		if (electrode1) 
			lastElectrode1 = gameTime.time;
		if (electrode2)
			lastElectrode2 = gameTime.time;

		var diff = Mathf.Abs (lastElectrode1 - lastElectrode2);
		if ( diff < 0.08) {
			if (!cocontracting) {
				this.rotate = !this.rotate;
				if (vibrateOnStateChange) {
					print ("state change");
					myo.Vibrate (Thalmic.Myo.VibrationType.Short);
				}
			}
			cocontracting = true;
		} else {
			cocontracting = false;
		}
	}

	protected override void _Awake () { 
		this.gameTime = FindObjectOfType<GameTime>();
		var windowSize = (int)Mathf.Ceil(filterWindowLengthInMilliseconds / (float)sampleTimeDifferenceInMilliseconds);
		this.filter1 = new Filter (windowSize);
		this.filter2 = new Filter (windowSize);
		timer = new Timer ();
		timer.Interval = sampleTimeDifferenceInMilliseconds;
		timer.Elapsed += (a, b) => {
			filter1.push (myo.emg [config.electrode1] / 127d);
			filter2.push (myo.emg [config.electrode2] / 127d);
		};
		timer.AutoReset = true;
	}


	void OnEnable() {
		timer.Enabled = true;
	}

	void OnDisable() {
		timer.Enabled = false;
	}

	override public float getRotationVelocity() {
		if (rotate) {
			var res = 0;
			if (electrodeClockWise) 
				res += 1;
			if (electrodeCounterClockWise)
				res -= 1;
			return res;
		} else {
			return 0;
		}
	}

	override public float getOpeningVelocity() {
		if (!rotate) {
			var res = 0;
			if (electrodeOpen)
				res += 1;
			if (electrodeClose) 
				res -= 1;
			return res;
		} else {
			return 0;
		}	
	}
}
 