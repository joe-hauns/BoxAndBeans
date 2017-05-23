using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
	public SpawningArea spawningArea{ private set; get; }
	public int gameDurationInSeconds = 60;

	private List<InvisibleWall> invisibleWalls;
	private List<OrdenaryWall> ordenaryWalls;
	private TargetArea targetArea;
	private GameLogic logic;

	private Spawner spawner;

	void Awake() {

		this.spawningArea = GetComponentInChildren<SpawningArea> ();
		this.invisibleWalls = new List<InvisibleWall> (GetComponentsInChildren<InvisibleWall> ());
		this.ordenaryWalls = new List<OrdenaryWall> (GetComponentsInChildren<OrdenaryWall> ());

		this.logic = FindObjectOfType<GameLogic> ();
		this.spawner = FindObjectOfType<Spawner> ();
		this.targetArea = GetComponentInChildren<TargetArea> ();

		Disable ();
	}

	public void Enable() {
		spawner.spawningArea = this.spawningArea;
		logic.level = this;
		invisibleWalls.ForEach (x => x.gameObject.SetActive (true));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (true));
		this.targetArea.gameObject.SetActive (true);
	}

	public void Disable() {
		invisibleWalls.ForEach (x => x.gameObject.SetActive (false));
		ordenaryWalls.ForEach (x => x.gameObject.SetActive (false));
		this.targetArea.gameObject.SetActive (false);
	}
}
