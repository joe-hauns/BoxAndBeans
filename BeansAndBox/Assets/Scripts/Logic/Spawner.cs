using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Spawner : MonoBehaviour
{

	public int spawnPackageSize = 3;
	public GameObject beanPrefab;
	public SpawningArea spawningArea{ private get; set; }

	public List<Bean> spawnedBeans { get; private set; }

	void Awake ()
	{
		this.spawnedBeans = new List<Bean> ();
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

			var spawnPoints = new List<Vector3> (spawnPackageSize);
			var spawnStart = Time.realtimeSinceStartup;

			for (int i = 0; i < spawnPackageSize; i++) {

				int attempts = 0;
				Vector3 point;
				do {
					attempts++;
					point = spawningArea.GetRandomPoint(minBorderDistance: spawnCenterDist / 2);

					if (Time.realtimeSinceStartup - spawnStart >= 0.5)
						throw new Exception ("Too much time elapsed for spawning beans. Maybe the spawning area is too small to place enough beans?");
				} while (
					spawnPoints.Exists (p => Mathf.Abs ((p - point).magnitude) <= spawnCenterDist)
					|| spawnedBeans.Exists (bean => Mathf.Abs ((bean.transform.position - point).magnitude) <= spawnCenterDist));
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
