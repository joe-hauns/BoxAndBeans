using System;
using UnityEngine;
using System.Collections.Generic;

public class Grabber : MonoBehaviour {

	private ProsthesisMovementController prosthesis;
	private bool grabberLocationInTargetArea = false;

	void Awake () {
		prosthesis = FindObjectOfType<ProsthesisMovementController> ();
	}

	void Start() {
		grabberLocationInTargetArea = false;
	}


	private void ApplyIfHas<Component>(Collider collider, Action<Component> action) {
		var obj = collider.gameObject.GetComponentInChildren<Component> ();
		if (obj != null) {
			action (obj);
		}
	}

	void OnTriggerEnter(Collider collider) {
		lock (this) {
			ApplyIfHas<Bean> (collider, bean => {
				if (collider == bean.grabable) {
					bean.transform.parent = prosthesis.transform;
					bean.transform.localPosition = new Vector3 (0, 0, 0);
					bean.transform.localRotation = Quaternion.Euler(new Vector3(0,90,90));
					bean.isGrabbed = true;
				}
			});
			ApplyIfHas<TargetArea> (collider, _ => {
				grabberLocationInTargetArea = true;
			});
		}
	}

	void OnTriggerExit(Collider collider) {
		lock (this) {
			ApplyIfHas<Bean> (collider, bean => {
				if (collider == bean.grabable) {
					bean.isGrabbed = false;
					bean.transform.parent = null;
					if (grabberLocationInTargetArea)
						bean.wasCollected = true;
				}
			});
			ApplyIfHas<TargetArea> (collider, _ => {
				grabberLocationInTargetArea = false;
			});
		}
	}

}
