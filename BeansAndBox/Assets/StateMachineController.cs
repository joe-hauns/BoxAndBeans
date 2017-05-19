using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pose = Thalmic.Myo.Pose;

namespace ProsthesisControllers {
	public class StateMachineController : MyoController {

		private bool rotate = true;
		private bool changedState = false;
		private float startCocontraction = -1;
		private GameTime gameTime;

		public override string controllerName { get { return "State Machine"; } }
		public override string defaultConfigFile { get{ return ""; } }
		public override bool needsConfiguration { get{ return false; } }
		public override ConfigResult SetConfiguration (string configPath) { return ConfigResult.OK; }

		protected override void _Update () { 
			if (myo.pose == Pose.FingersSpread || myo.pose == Pose.Fist) {
				if (startCocontraction == -1) {
					startCocontraction = gameTime.time;
				} else if (0.080 < gameTime.time - startCocontraction) {
					if (!changedState) {
						this.rotate = !this.rotate;
						myo.Vibrate (Thalmic.Myo.VibrationType.Short);
						changedState = true;
					}
				}
			} else {
				startCocontraction = -1;
				changedState = false;
			}
		}

		protected override void _Awake () { 
			this.gameTime = FindObjectOfType<GameTime>();
		}

		override public float getRotationVelocity() {
			if (rotate) {
				if (myo.pose == Pose.WaveOut)
					return 1;
				else if (myo.pose == Pose.WaveIn) 
					return -1;
				else
					return 0;
			} else {
				return 0;
			}
		}

		override public float getOpeningVelocity() {
			if (!rotate) {
				if (myo.pose == Pose.WaveOut)
					return 1;
				else if (myo.pose == Pose.WaveIn) 
					return -1;
				else
					return 0;
			} else {
				return 0;
			}	
		}
	}
}