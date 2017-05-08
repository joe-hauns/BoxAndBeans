using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EsnDebugUiValue : MonoBehaviour {

	private RectTransform _value;
	private Text _valueText;

	public double value {
		set {
			if (value < 0) {
				_value.anchorMin = new Vector2( (float)(.5 +  value / 2.0), 0.5f);
				_value.anchorMax = new Vector2( 0.5f, 0.5f);
			} else {
				_value.anchorMin = new Vector2( 0.5f, 0.5f);
				_value.anchorMax = new Vector2( (float) (.5 +  value / 2.0), 0.5f);
			}
			_valueText.text = "" + ((int)(1000 * value) / 1000f);
		}
	}

	// Use this for initialization
	void Awake () {
		var trafos = new List<RectTransform> (GetComponentsInChildren<RectTransform> ());
		this._value = trafos.Find (x => x.name == "Value");

		var texts = new List<Text> (GetComponentsInChildren<Text> ());
		this._valueText = texts.Find (x => x.name == "ValueText");
	}
	
}
