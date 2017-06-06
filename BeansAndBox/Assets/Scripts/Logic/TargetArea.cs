using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used only to identify the Target Area game object instead of using a tag.
/// </summary>
public class TargetArea : MonoBehaviour {
	private Dictionary<Bean, HashSet<Collider>> beans;

	void Start() {
		this.beans = new Dictionary<Bean,HashSet<Collider>> ();
	}

	void OnTriggerEnter(Collider c) {
		var bean = c.GetComponent<Bean> ();
		if (bean != null) {
			if (!beans.ContainsKey (bean))
				beans[bean] = new HashSet<Collider> ();
			beans[bean].Add (c);
		}
	}

	void OnTriggerExit(Collider c) {
		var bean = c.GetComponent<Bean> ();
		if (bean != null) {
			beans [bean].Remove (c);
			if (beans [bean].Count == 0) {
				beans.Remove (bean);
				bean.wasCollected = false;
			}
		}
	}
}
