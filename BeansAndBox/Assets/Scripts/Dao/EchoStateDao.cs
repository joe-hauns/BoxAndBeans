using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EchoState;
using System.IO;

public class EchoStateDao : ControllerLoader {
	void fn() {
		Dropdown d;
		//d.RefreshShownValue ();
	}
/*
	public string configDir{ get; private set; }

	void Awake() {
		configDir = Path.Combine(Application.persistentDataPath, "EchoStateNetwork");
		if (!Directory.Exists (configDir)) {
			Directory.CreateDirectory (configDir);
		}
	}

	public override ProsthesisController Load() {
		var resDir = Path.Combine (configDir, "current_reservoir.json");
		var outDir = Path.Combine (configDir, "current_output_weights.json");

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
*/
}
