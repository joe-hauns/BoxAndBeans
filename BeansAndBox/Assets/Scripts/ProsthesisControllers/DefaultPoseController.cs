using UnityEngine;
using System.Collections;

using Pose = Thalmic.Myo.Pose;
namespace ProsthesisControllers {
	public class DefaultPoseController : MyoController {

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
}