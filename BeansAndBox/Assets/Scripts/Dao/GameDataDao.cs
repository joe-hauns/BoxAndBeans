using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class GameDataDao : MonoBehaviour {

	private string savesDir;

	void Awake() {
		savesDir = Path.Combine (Application.persistentDataPath, "Saves");
		if (!Directory.Exists (savesDir))
			Directory.CreateDirectory (savesDir);
	}

	public void Save(GameStateDto dto) {
		string json = JsonUtility.ToJson(dto);
		var path = Path.Combine (savesDir, dto.participant + "_" + dto.dateTime.ToString ("yyyy-MM-dd_hh-mm-ss") + ".json");

		if (path.Length != 0) {
			using (var writer = new System.IO.StreamWriter (path)) {
				writer.WriteLine (json.ToString ());
			}
		}
	}

	public List<GameStateDto> AllGames() {
		return new List<string> (Directory.GetFiles (savesDir)) .ConvertAll (file => {
			return JsonUtility.FromJson<GameStateDto> (File.ReadAllText (file));
		});
	}
}
