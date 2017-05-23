using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class GameDataDao : MonoBehaviour {

	private string _savesDir;
	public string persistenceDir { 
		private get{ return _savesDir; } 
		set {
			_savesDir = value;
			if (!Directory.Exists (value))
				Directory.CreateDirectory (value);
		}
	}

		//savesDir = Path.Combine (Application.persistentDataPath, "Scores");

	public void Save(GameStateDto dto) {
		string json = JsonUtility.ToJson(dto);
		var path = Path.Combine (persistenceDir, dto.participant + "_" + dto.dateTime.ToString ("yyyy-MM-dd_hh-mm-ss") + ".json");

		if (path.Length != 0) {
			using (var writer = new System.IO.StreamWriter (path)) {
				writer.WriteLine (json.ToString ());
			}
		}
	}

	public List<GameStateDto> AllGames() {
		return new List<string> (Directory.GetFiles (persistenceDir)) .ConvertAll (file => {
			return JsonUtility.FromJson<GameStateDto> (File.ReadAllText (file));
		});
	}
}
