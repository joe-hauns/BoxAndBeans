using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoStateDebugUI : MonoBehaviour {

	public EsnDebugUiGroup normalized { get; private set;}
	public EsnDebugUiGroup raw {get; private set;}

	// Use this for initialization
	void Awake () {
		var groups = new List<EsnDebugUiGroup> (GetComponentsInChildren<EsnDebugUiGroup> ());
		this.normalized = groups.Find (x => x.name == "Normalized");
		this.raw = groups.Find (x => x.name == "Raw");
	}
}
