using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
	public SpawningArea spawningArea{ private set; get; }
	public int gameDurationInSeconds = 60;
	public static int ids;
	public int id;

	private List<InvisibleWall> invisibleWalls;
	private List<OrdenaryWall> ordenaryWalls;
	private TargetArea targetArea;
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
			this.targetArea = GetComponentInChildren<TargetArea> ();
			if (targetArea == null)
				throw new System.Exception ();

			this.didInit = true;
		}
	}

	public void Enable() {
		Awake ();
		spawner.spawningArea = this.spawningArea;
		logic.level = this;
		testLogic.vanishingTimeInSec = gameDurationInSeconds;
		invisibleWalls.ForEach (x => x.gameObject.SetActive (true));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (true));
		this.targetArea.gameObject.SetActive (true);
	}

	public void Disable() {
		Awake ();
		invisibleWalls.ForEach (x => x.gameObject.SetActive (false));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (false));
		this.targetArea.gameObject.SetActive (false);
	}
}
