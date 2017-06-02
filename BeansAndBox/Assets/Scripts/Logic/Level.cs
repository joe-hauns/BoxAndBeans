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
	private bool didAwake = false;

	void Awake() {
		print ("awake "+this.name);
		this.spawningArea = GetComponentInChildren<SpawningArea> ();
		this.invisibleWalls = new List<InvisibleWall> (GetComponentsInChildren<InvisibleWall> ());
		this.ordenaryWalls = new List<OrdenaryWall> (GetComponentsInChildren<OrdenaryWall> ());

		this.testLogic = FindObjectOfType<TestRunLogic> ();
		this.logic = FindObjectOfType<GameLogic> ();
		this.spawner = FindObjectOfType<Spawner> ();
		this.targetArea = GetComponentInChildren<TargetArea> ();

		this.didAwake = true;
		//Disable ();
	}

	public void Enable() {
		if (!didAwake)
			Awake ();
		print ("enable "+this.name);
		spawner.spawningArea = this.spawningArea;
		logic.level = this;
		testLogic.vanishingTimeInSec = gameDurationInSeconds;
		invisibleWalls.ForEach (x => x.gameObject.SetActive (true));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (true));
		this.targetArea.gameObject.SetActive (true);
	}

	public void Disable() {
		if (!didAwake)
			Awake ();
		print ("disable "+this.name);
		invisibleWalls.ForEach (x => x.gameObject.SetActive (false));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (false));
		this.targetArea.gameObject.SetActive (false);
	}
}
