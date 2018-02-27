#define EVENT_BASED_EMG
using UnityEngine;
using System.Collections;
using EchoState;
using System.IO;
using GMLVQ;
using SimpleJSON;

public class GMLVQController : MyoController {

	public override string controllerName { get { return "GMLVQ Model"; } }
	public override string defaultConfigFile { get{ return Path.Combine(Application.persistentDataPath, "gmlvq"); } }
	public override bool needsConfiguration { get{ return true; } }
	public override ConfigResult SetConfiguration (string configPath) {
		var modelPath = Path.Combine (configPath, "gmlvq_model.json");
		var transformPath = Path.Combine (configPath, "transfer_mapping.json");

		// Try to read the transform matrix
		if(File.Exists(transformPath)) {
			Debug.LogWarning("start loading GMLVQ with transfer mapping");
			var raw_data_string = FileRead.readFileData(transformPath);
			this.trafoMatrix = DenseMatrix.FromJSON((JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["transfMap"]);
		} else {
			Debug.LogWarning("start loading GMLVQ without transfer mapping");
			// if the file for the transform matrix does not exist, we use an identity matrix
			this.trafoMatrix = null;
		}

		if (!File.Exists (modelPath)) {
			return ConfigResult.error("Resource file missing: " + modelPath);
		} else {
			try {
				// parse multiple GMLVQ models, one per class.
				this.models = GMLVQModel.MultiFromJSON (modelPath);
				if(this.models.Length != 2) {
					return ConfigResult.error("Expected two GMLVQ models!");
				}
				return ConfigResult.OK;
			} catch (System.Exception e) {
				return ConfigResult.error(e.Message);
			}
		}
	}

	private Buffer buffer = new Buffer();
	private CombFilter filter = new CombFilter(200, 50);
	private GMLVQModel[] models;
	private double[,] trafoMatrix;

	private float openVelo = 0;
	private float rotationVelo = 0;

	protected override void _Awake() {
		this.enabled = false;
	}


	#if EVENT_BASED_EMG

	void OnEnable() {
		myo.EmgEvent += emg_changed;
	}
	void OnDisable() {
		myo.EmgEvent -= emg_changed;
	}

	protected override void _Update() { }

	public const float dt = 0.45f;

	void emg_changed (object sender, Thalmic.Myo.EmgDataEventArgs e) {
		if (buffer.store (e.Emg)) {
	#else
	protected override void _Update() {
		if (myo.isPaired && buffer.store (myo.emg)) {
	#endif
			var features = Features.extractFeatures (filter.filter (buffer.retrieve ()));
			if(trafoMatrix != null) {
				features = DenseMatrix.multiply(trafoMatrix, features);
			}

			int openLabel;
			double openConfidence;
			models[0].Confidence(features, out openLabel, out openConfidence);

			int rotationLabel;
			double rotationConfidence;
			models[1].Confidence(features, out rotationLabel, out rotationConfidence);

			openVelo     = dt * ((float) openLabel) * 0.8f + (1 - dt) * openVelo;
			if(openLabel != 0) {
				if(rotationConfidence < 0.5) {
					rotationLabel = 0;
				}
			}
			rotationVelo = dt * ((float) rotationLabel) * 0.8f + (1 - dt) * rotationVelo;
		}
	}

	override public float OpeningVelocity() {
		return openVelo;
	}

	override public float RotationVelocity() {
		return rotationVelo;
	}
}
