using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SettingsDao : MonoBehaviour {
	string settingsFile;

	// Use this for initialization
	void Awake () {
		this.settingsFile = Path.Combine (Application.persistentDataPath, "settings.json");
	}

	public void Save(SettingsDto dto) {
		string json = JsonUtility.ToJson(dto);

		using (var writer = new System.IO.StreamWriter (settingsFile)) {
			writer.WriteLine (json.ToString ());
		}
	}

	public SettingsDto Load() {
		try {
			return JsonUtility.FromJson<SettingsDto> (File.ReadAllText (settingsFile));
		} catch (System.Exception ) {
			return null;
		}
	}
}
