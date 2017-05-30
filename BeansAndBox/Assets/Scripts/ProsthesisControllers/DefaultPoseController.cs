using UnityEngine;
using System.Collections;

using Pose = Thalmic.Myo.Pose;
public class DefaultPoseController : MyoController {

	public override string controllerName { get { return "Thalmic Pose"; } }
	public override string defaultConfigFile { get{ return ""; } }
	public override bool needsConfiguration { get{ return false; } }
	public override ConfigResult SetConfiguration (string configPath) { return ConfigResult.OK; }

	protected override void _Update () { }
	protected override void _Awake () { }

	override public float getRotationVelocity() {
		if (myo.pose == Pose.WaveOut)
			return 1;
		else if (myo.pose == Pose.WaveIn) 
			return -1;
		else
			return 0;
	}

	override public float getOpeningVelocity() {
		if (myo.pose == Pose.FingersSpread)
			return 1;
		else if (myo.pose == Pose.Fist) 
			return -1;
		else
			return 0;
	}
}
