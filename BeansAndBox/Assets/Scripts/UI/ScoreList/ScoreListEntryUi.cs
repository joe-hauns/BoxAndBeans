using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreListEntryUi : MonoBehaviour
{
	private Text _name;
	private Text _score;
	private TextStyles fonts;

	public string title { set { _name.text = value; } }

	public int score { set { _score.text = "" + value; } }

	void Awake ()
	{
		var texts = new List<Text> (GetComponentsInChildren<Text> ());
		this._name = texts.Find (t => t.name == "Name");
		this._score = texts.Find (t => t.name == "Score");
		this.fonts = FindObjectOfType<TextStyles> ();
	}

	public bool highlighted {
		set {
			if (value) {
				_name.font = fonts.bold;
				_score.font = fonts.bold;
			} else {
				_name.font = fonts.regular;
				_score.font = fonts.regular;
			}
		}
	}

	public void Clear () {
		_name.text = "";
		_score.text = "";
	}

}
