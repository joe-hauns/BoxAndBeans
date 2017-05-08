using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EchoState;
using System.IO;

public class EchoStateDao : ControllerLoader {
	private string esnDir;

	void Awake() {
		esnDir = Path.Combine(Application.persistentDataPath, "EchoStateNetwork");
		if (!Directory.Exists (esnDir)) {
			Directory.CreateDirectory (esnDir);
		}
	}

	public override ProsthesisController Load() {
		var resDir = Path.Combine (esnDir, "current_reservoir.json");
		var outDir = Path.Combine (esnDir, "current_output_weights.json");

		var ctrl = FindObjectOfType<ProsthesisControllers.EchoStateController> ();
		if (!File.Exists (resDir) || !File.Exists (outDir)) {
			Debug.LogWarning ("Resource file missing: " + (!File.Exists (outDir) ? outDir : resDir));
			ctrl.enabled = false;
			return null;
		} else {
			ctrl.esn = EchoStateNetwork.FromJSON (resDir, outDir);
			return ctrl;
		}
	}
}
