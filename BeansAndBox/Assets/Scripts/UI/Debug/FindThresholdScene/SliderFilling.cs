using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderFilling : MonoBehaviour {
	private Image green;
	private RectTransform rect;

	public float value {
		set {
			rect.anchorMax = new Vector2 (value, rect.anchorMax.y);
		}
	}
	public bool highlighted {
		set {
			green.gameObject.SetActive (value);
		}
	}

	// Use this for initialization
	void Start () {
		var lsts = new List<Image> (GetComponentsInChildren<Image> ());
		this.green = lsts.Find (x => x.gameObject.name == "Green");
		this.rect = GetComponent<RectTransform> ();
		highlighted = false;
	}
}
