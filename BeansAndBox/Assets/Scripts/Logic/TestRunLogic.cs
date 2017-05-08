using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestRunLogic : MonoBehaviour {

	/// <summary> The time it takes until a bean vanishes after it was collected. </summary>
	public float vanishingTimeInSec = 10;

	private Spawner spawner;
	private Action onTermination;
	private TestRunUi ui;
	private GameTime gameTime;

	// Use this for initialization
	void Awake () {
		this.spawner = FindObjectOfType<Spawner> ();
		this.enabled = false;
		this.ui = GetComponentInChildren<TestRunUi> ();
		this.gameTime = FindObjectOfType<GameTime> ();
	}

	public void Launch(Action onTermination) {
		this.onTermination = onTermination;
		this.enabled = true;
		this.spawner.ClearBeans ();
		this.ui.Show ();
		gameTime.Reset ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Escape)) {
			Exit ();
		}

		lock (spawner) {
			var toDestroy = spawner.spawnedBeans.FindAll (b => b.wasCollected && gameTime.time - b.timeCollected >= vanishingTimeInSec);
			spawner.spawnedBeans.RemoveAll(b => b.wasCollected && gameTime.time - b.timeCollected >= vanishingTimeInSec);
			toDestroy.ForEach (b => b.Destroy());
		}
	}

	public void Exit() {
		this.enabled = false;
		spawner.ClearBeans ();
		this.ui.Hide ();
		onTermination ();
	}
}
