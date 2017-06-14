using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAreas : MonoBehaviour {

	private List<TargetArea> areas;

	// Use this for initialization
	void Awake () {
		this.areas = new List<TargetArea>(GetComponentsInChildren<TargetArea> ());
		foreach (var area in areas) {
			area.parent = this;
		}
	}
	
	public void BeanExited (Bean b)
	{
		print ("bean  exit");
		if (areas.TrueForAll (a => !a.Contains (b))) {
			b.wasCollected = false;
			print ("wasCollected := false");
		}
	}
}
