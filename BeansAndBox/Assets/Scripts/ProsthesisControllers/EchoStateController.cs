//#define DEBUG_ESN
#define EVENT_BASED_EMG
using UnityEngine;
using System.Collections;
using EchoState;
using System.IO;

public class EchoStateController : MyoController {

	public override string controllerName { get { return "Echo State Network"; } }
	public override string defaultConfigFile { get{ return Path.Combine(Application.persistentDataPath, "EchoStateNetwork"); } }
	public override bool needsConfiguration { get{ return true; } }
	public override ConfigResult SetConfiguration (string configPath) {
		var resFile   = Path.Combine (configPath, "current_reservoir.json");
		var transFile = Path.Combine (configPath, "transfer_mapping.json");
		var outFile   = Path.Combine (configPath, "current_output_weights.json");

		if (!File.Exists (resFile) || !File.Exists (outFile)) {
			return ConfigResult.error("Resource file missing: " + (!File.Exists (outFile) ? outFile : resFile));
		} else {
			if(!File.Exists(transFile)) {
				Debug.LogWarning("start loading ESN without transfer mapping");
				try {
					this.esn = EchoStateNetwork.FromJSON (resFile, outFile);
					return ConfigResult.OK;
				} catch (System.Exception e) {
					return ConfigResult.error(e.Message);
				}
			} else {
				Debug.LogWarning("start loading ESN with transfer mapping");
				try {
					this.esn = EchoStateNetwork.FromJSON (resFile, transFile, outFile);
					return ConfigResult.OK;
				} catch (System.Exception e) {
					return ConfigResult.error(e.Message);
				}
			}
		}
	}

	private Buffer buffer = new Buffer();
	private CombFilter filter = new CombFilter(200, 50);
	private EchoStateNetwork esn;


	#if UNITY_EDITOR && DEBUG_ESN
	private EchoStateDebugUI debug;
	#endif

	private float openVelo = 0;
	private float rotationVelo = 0;

	protected override void _Awake() {
		var dbg = GetComponentInChildren<EchoStateDebugUI>();
		this.enabled = false;
		#if UNITY_EDITOR && DEBUG_ESN
		dbg.gameObject.SetActive(true);
		this.debug = dbg;
		#else
		dbg.gameObject.SetActive(false);
		#endif
	}

	#if EVENT_BASED_EMG

	void OnEnable() {
		myo.EmgEvent += emg_changed;
	}
	void OnDisable() {
		myo.EmgEvent -= emg_changed;
	}

	protected override void _Update() { }

	void emg_changed (object sender, Thalmic.Myo.EmgDataEventArgs e) {
		if (buffer.store (e.Emg)) {
	#else
	protected override void _Update() {
		if (myo.isPaired && buffer.store (myo.emg)) {
	#endif
			esn.Update (Features.extractFeatures(filter.filter (buffer.retrieve ())));
			var output = esn.GetNormalizedOutput ();
			openVelo = (float) output [0];
			rotationVelo = (float) output [1];
			#if UNITY_EDITOR && DEBUG_ESN
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

	override public float OpeningVelocity() {
		return openVelo;
	}

	override public float RotationVelocity() {
		return rotationVelo;
	}
}
