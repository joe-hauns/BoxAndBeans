using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUi : MonoBehaviour {

	private Text _time;
	private Text _score;
	private Canvas canvas;

	public int score { set { _score.text = "" + value; }}
	public float time { set { _time.text = "" + (int)value; }}


	// Use this for initialization
	void Awake () {
		var texts = new List<Text> (GetComponentsInChildren <Text> ());
		this._time = texts.Find (t => t.name == "Time");
		this._score = texts.Find (t => t.name == "Score");
		this.canvas = GetComponent<Canvas> ();
	}

	void Start () {
		canvas.enabled = false;
	}

	public void Show() {
		canvas.enabled = true;
	}

	public void Hide() {
		canvas.enabled = false;
	}
}
