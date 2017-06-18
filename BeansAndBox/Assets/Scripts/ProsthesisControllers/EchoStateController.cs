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
		var resDir = Path.Combine (configPath, "current_reservoir.json");
		var outDir = Path.Combine (configPath, "current_output_weights.json");

		if (!File.Exists (resDir) || !File.Exists (outDir)) {
			return ConfigResult.error("Resource file missing: " + (!File.Exists (outDir) ? outDir : resDir));
		} else {
			try {
				this.esn = EchoStateNetwork.FromJSON (resDir, outDir);
				return ConfigResult.OK;
			} catch (System.Exception e) {
				return ConfigResult.error(e.Message);
			}
		}
	}

	private Buffer buffer = new Buffer();
	private CombFilter filter = new CombFilter(200, 50);
	private EchoStateNetwork esn;


	#if UNITY_EDITOR
	private EchoStateDebugUI debug;
	#endif

	private float openVelo = 0;
	private float rotationVelo = 0;

	protected override void _Awake() {
		var dbg = GetComponentInChildren<EchoStateDebugUI>();
		this.enabled = false;
		#if UNITY_EDITOR
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

	override public float OpeningVelocity() {
		return openVelo;
	}

	override public float RotationVelocity() {
		return rotationVelo;
	}
}
