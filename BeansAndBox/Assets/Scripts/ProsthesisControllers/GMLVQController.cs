using UnityEngine;
using System.Collections;
using EchoState;
using System.IO;
using GMLVQ;
using SimpleJSON;

public class GMLVQController : MyoController {

	public override string controllerName { get { return "GMLVQ Model"; } }
	public override string defaultConfigFile { get{ return Path.Combine(Application.persistentDataPath, "QMLVQ"); } }
	public override bool needsConfiguration { get{ return true; } }
	public override ConfigResult SetConfiguration (string configPath) { 
		var modelPath = Path.Combine (configPath, "qmlvq.json");
		var transformPath = Path.Combine (configPath, "transform.json");

		if (!File.Exists (modelPath) || !File.Exists (transformPath)) {
			return ConfigResult.error("Resource file missing: " + (!File.Exists(modelPath) ? modelPath : transformPath));
		} else {
			try {
				var raw_data_string = FileRead.readFileData(transformPath);
				this.trafoMatrix = DenseMatrix.FromJSON((JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["transform"]);

				this.model = GMLVQModel.FromJSON (configPath);

				return ConfigResult.OK;
			} catch (System.Exception e) {
				return ConfigResult.error(e.Message);
			}
		}
	}

	private Buffer buffer = new Buffer();
	private CombFilter filter = new CombFilter(200, 50);
	private GMLVQModel model;
	private double[,] trafoMatrix;

	private float openVelo = 0;
	private float rotationVelo = 0;

	protected override void _Awake() {
		this.enabled = false;
	}

	protected override void _Update() {
		if (myo.isPaired && buffer.store (myo.emg)) {
			var features = Features.extractFeatures (filter.filter (buffer.retrieve ()));
			var output = model.GetOutput (DenseMatrix.multiply (trafoMatrix, features));
			openVelo = (float) output [0];
			rotationVelo = (float) output [1];
		}
	}

	override public float OpeningVelocity() {
		return openVelo;
	}

	override public float RotationVelocity() {
		return rotationVelo;
	}
}
