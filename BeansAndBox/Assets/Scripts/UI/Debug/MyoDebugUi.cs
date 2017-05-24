using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyoDebugUi : MonoBehaviour {
	public bool filtered = false;
	public int filterSize = 10;

	#if UNITY_EDITOR
	private ThalmicMyo myo;
	private EsnDebugUiValue[] emgValues;
	private Filter[] filters;

	void Awake () {
		this.myo = GetComponentInParent<ThalmicMyo> ();
		this.emgValues = GetComponentsInChildren<EsnDebugUiValue> ();
		this.filters = new Filter[8];
		for (int i = 0; i < filters.Length; i++) {
			filters [i] = new Filter (filterSize);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (myo.isPaired) {
			var emg = myo.emg;
			for (int i = 0; i < emg.Length; i++) {
				if (filtered) {
					filters [i].push ((double)emg [i] / 128d);
					emgValues [i].value = filters [i].get ();
				} else {
					emgValues [i].value = (double)emg [i] / 128d;
				}
			}
		}
	}


	#else
	void Awake () {
		gameObject.SetActive (false);
	}
	#endif
}

