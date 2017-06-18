using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System;
using System.Reflection;

public class EmgThresholdDebugUi : MonoBehaviour {
	private EmgSliderRow[] sliders;
	private ThalmicMyo myo;
	private Filter[] filters;
	public int sampleTimeDifferenceInMilliseconds = 10;
	public int filterWindowLengthInMilliseconds = 150;
	private Timer timer;

	// Use this for initialization
	void Start () {
		this.sliders = GetComponentsInChildren<EmgSliderRow> ();
		this.myo = FindObjectOfType<ThalmicMyo> ();
	
		var windowSize = (int)Mathf.Ceil(filterWindowLengthInMilliseconds / (float)sampleTimeDifferenceInMilliseconds);
		this.filters = new Filter[myo.emg.Length];
		for (int i = 0; i < filters.Length; i++) {
			filters [i] = new Filter (windowSize);
			sliders [i].caption = "EMG [ " + i + " ]";
		}
		timer = new Timer ();
		timer.Interval = sampleTimeDifferenceInMilliseconds;
		timer.Elapsed += (a, b) => {
			for (int i = 0; i < filters.Length; i++) {
				filters[i].push (myo.emg [i] / 127d);
			}
		};
		timer.AutoReset = true;
		timer.Enabled = true;
	}

	void Update() { 
		for (int i = 0; i < filters.Length; i++) {
			sliders [i].value = (float)filters [i].get ();
		}
	}
}
