using UnityEngine;
using System.Collections;
using EchoState;
using System.IO;

namespace ProsthesisControllers {
	public class EchoStateController : MyoController {

		private Buffer buffer = new Buffer();
		private CombFilter filter = new CombFilter(200, 50);
		public EchoStateNetwork esn{ set; private get;}


		#if UNITY_EDITOR
		private EchoStateDebugUI debug;
		#endif
	
		private float openVelo = 0;
		private float rotationVelo = 0;

		protected override void _Awake() {
			var dbg = GetComponentInChildren<EchoStateDebugUI>();
			#if UNITY_EDITOR
			dbg.gameObject.SetActive(true);
			this.debug = dbg;
			#else
			dbg.gameObject.SetActive(false);
			#endif
		}

		protected override void _Update() {
			if (myo.isPaired && buffer.store (myo.emg)) {
				esn.Update (Features.extractFeatures(filter.filter (buffer.retrieve ())));
				var output = esn.GetNormalizedOutput ();
				openVelo = (float) output [0];
				rotationVelo = (float) output [1];
				#if UNITY_EDITOR
				{
					var norm = esn.GetNormalizedOutput();
					this.debug.normalized.x.value = norm[0];
					this.debug.normalized.y.value = norm[1];
					var raw = esn.GetOutput();
					this.debug.raw.x.value = raw[0];
					this.debug.raw.y.value = raw[1];
				}
				#endif
			}

		}

		override public float getOpeningVelocity() {
			return openVelo;
		}

		override public float getRotationVelocity() {
			return rotationVelo;
		}
	}
}