using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Spawner : MonoBehaviour
{

	public int spawnPackageSize = 3;
	public GameObject beanPrefab;
	private BoxCollider2D spawningArea;

	public List<Bean> spawnedBeans { get; private set; }

	void Awake ()
	{
		this.spawnedBeans = new List<Bean> ();
		this.spawningArea = new List<BoxCollider2D> (GetComponentsInChildren <BoxCollider2D> ()).Find (c => c.name == "SpawningArea");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (spawnedBeans.TrueForAll (b => b.wasCollected))
			SpawnBeans ();
	}

	/// <summary>
	/// Spawns new beans at random positions within the spawning area. The rotation
	/// of the bean will be randomized. The number of beans spawned can be set with the 
	/// field <see cref="spawnPackageSize"/>.
	/// </summary>
	private void SpawnBeans ()
	{
		lock (this) {
			var spawnCenterDist = beanPrefab.GetComponent<Bean> ().MaxDiameter;
			Bounds b = new Bounds ();
			b.min = spawningArea.bounds.min + new Vector3 (1, 1, 0) * spawnCenterDist / 2;
			b.max = spawningArea.bounds.max - new Vector3 (1, 1, 0) * spawnCenterDist / 2;
			var range = (b.max - b.min);

			var spawnPoints = new List<Vector3> (spawnPackageSize);
			var spawnStart = Time.realtimeSinceStartup;

			for (int i = 0; i < spawnPackageSize; i++) {

				int attempts = 0;
				Vector3 point;
				do {
					attempts++;
					point = b.min;
					point.x += range.x * UnityEngine.Random.value;
					point.y += range.y * UnityEngine.Random.value;
					if (Time.realtimeSinceStartup - spawnStart >= 0.5)
						throw new Exception ("Too much time elapsed for spawning beans. Maybe the spawning area is too small to place enough beans?");
				} while (
					spawnPoints.Exists (p => Mathf.Abs ((p - point).magnitude) <= spawnCenterDist)
					|| spawnedBeans.Exists (bean => Mathf.Abs ((bean.transform.position - point).magnitude) <= spawnCenterDist));
				//if (attempts != 1) print ("attempts: " + attempts);
				spawnPoints.Add (point);
			}

			foreach (var spawnXY in spawnPoints) {
				var rotation = 180f * UnityEngine.Random.value;
				var bean = Instantiate (beanPrefab, spawnXY, Quaternion.Euler (rotation, -90, 0)).GetComponent<Bean> ();
				spawnedBeans.Add (bean);
			}
		}
	}

	public int collectedBeanCount ()
	{
		lock (this) {
			int c = 0;
			foreach (Bean b in spawnedBeans)
				if (b.wasCollected)
					c++;
			return c;
		}
	}

	public void ClearBeans ()
	{
		lock (this) {
			/* cleaning up the last run */
			foreach (Bean b in spawnedBeans)
				b.Destroy ();
			spawnedBeans = new List<Bean> ();
		}
	}
}
