using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningArea : MonoBehaviour {
	private List<Bounds> bounds;

	void Awake() 
	{
		var areas = new List<BoxCollider2D> (GetComponentsInChildren<BoxCollider2D> ());
		this.bounds = areas.ConvertAll (a => a.bounds);
	}

	public Vector2 GetRandomPoint(float minBorderDistance) 
	{
		var bounds = this.bounds.FindAll(b => {
			var r = b.max - b.min;
			return r.x >= 2*minBorderDistance && r.y >= 2*minBorderDistance;
		});
		var chosenBounds = bounds [((int)(UnityEngine.Random.value * bounds.Count)) % bounds.Count];

		Bounds bound = new Bounds ();
		bound.min = chosenBounds.min + new Vector3 (1, 1, 0) * minBorderDistance;
		bound.max = chosenBounds.max - new Vector3 (1, 1, 0) * minBorderDistance;
		var range = (bound.max - bound.min);

		var point = bound.min;
		point.x += range.x * UnityEngine.Random.value;
		point.y += range.y * UnityEngine.Random.value;
		return point;
	}
}
