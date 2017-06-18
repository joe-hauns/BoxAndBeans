using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmgSliderRow : MonoBehaviour {
	private Slider slider;
	private SliderFilling filling;
	private Text text;
	private Text label;

	public string caption {
		set { label.text = value; }
	}

	private double threshold {
		get { return slider.normalizedValue; }
	}

	public float value {
		set {
			filling.highlighted = value >= threshold;
			filling.value = value;
		}
	}

	// Use this for initialization
	void Awake () {
		this.slider = GetComponentInChildren<Slider> ();
		this.filling = GetComponentInChildren<SliderFilling> ();
		var texts = new List<Text> (GetComponentsInChildren<Text> ());
		this.text = texts.Find (txt => txt.name == "Value");
		this.label = texts.Find (txt => txt.name == "Label");
		this.slider.value = 1.0f;
	}

	public void SliderValueChanged() {
		text.text = "" + ((int)(threshold * 1000)) / 1000f;
	}
}
