using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyoDebugUi : MonoBehaviour {
	#if UNITY_EDITOR
	private ThalmicMyo myo;
	private EsnDebugUiValue[] emgValues;

	void Awake () {
		this.myo = GetComponentInParent<ThalmicMyo> ();
		this.emgValues = GetComponentsInChildren<EsnDebugUiValue> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (myo.isPaired) {
			var emg = myo.emg;
			for (int i = 0; i < emg.Length; i++) {
				emgValues [i].value = (double) emg [i] / 128d;
			}
		}
	}

	#else
	void Awake () {
		gameObject.SetActive (false);
	}
	#endif
}
