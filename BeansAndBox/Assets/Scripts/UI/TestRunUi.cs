using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRunUi : MonoBehaviour {

	private Canvas canvas;

	// Use this for initialization
	void Awake () {
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
