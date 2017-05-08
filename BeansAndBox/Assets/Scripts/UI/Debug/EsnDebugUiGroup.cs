using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsnDebugUiGroup : MonoBehaviour {

	public EsnDebugUiValue x{ get; private set;}
	public EsnDebugUiValue y{ get; private set;}

	// Use this for initialization
	void Awake () {
		var vals = new List<EsnDebugUiValue> (GetComponentsInChildren<EsnDebugUiValue> ());
		this.x = vals.Find (x => x.name == "X");
		this.y = vals.Find (y => y.name == "Y");
	}
}
