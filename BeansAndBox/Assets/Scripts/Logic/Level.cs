using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
	public int gameDurationInSeconds = 60;
	public int spawnPackageSize = 3;

	private SpawningArea spawningArea{ set; get; }
	private List<InvisibleWall> invisibleWalls;
	private List<OrdenaryWall> ordenaryWalls;
	private TargetAreas targetAreas;
	private GameLogic logic;
	private TestRunLogic testLogic;

	private Spawner spawner;
	private bool didInit = false;

	void Awake() {
		if (!didInit) {
			this.spawningArea = GetComponentInChildren<SpawningArea> ();
			this.invisibleWalls = new List<InvisibleWall> (GetComponentsInChildren<InvisibleWall> ());
			this.ordenaryWalls = new List<OrdenaryWall> (GetComponentsInChildren<OrdenaryWall> ());

			this.testLogic = FindObjectOfType<TestRunLogic> ();
			this.logic = FindObjectOfType<GameLogic> ();
			this.spawner = FindObjectOfType<Spawner> ();
			this.targetAreas = GetComponentInChildren<TargetAreas> ();
			if (targetAreas == null)
				throw new System.Exception ();

			this.didInit = true;
		}
	}

	public void Enable() {
		Awake ();
		spawner.spawningArea = this.spawningArea;
		spawner.spawnPackageSize = this.spawnPackageSize;
		logic.level = this;
		testLogic.vanishingTimeInSec = gameDurationInSeconds;
		invisibleWalls.ForEach (x => x.gameObject.SetActive (true));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (true));
		this.targetAreas.gameObject.SetActive (true);
	}

	public void Disable() {
		Awake ();
		invisibleWalls.ForEach (x => x.gameObject.SetActive (false));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (false));
		this.targetAreas.gameObject.SetActive (false);
	}
}
