using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

public class GameLogic : MonoBehaviour
{

	public int gameDurationInSec { set; private get;}

	/* References to objects that are found automatically in the unity hierarchy. */
	private ScoreUi ui;
	private Spawner spawner;
	private GameTime gameTime;

	/* State of the current game run. */
	private string player;
	private Action<GameStateDto> onTermination;
	private ProsthesisMovementController controller;
	private List<ControllerStateDto> controllerStates;

	void Awake () {
		this.ui = GetComponentInChildren<ScoreUi> ();
		this.controller = FindObjectOfType<ProsthesisMovementController> ();
		this.spawner = FindObjectOfType<Spawner> ();
		this.gameTime = FindObjectOfType<GameTime> ();
	}

	void Start () {
		this.enabled = false;
	}

	public void Launch (string player, Action<GameStateDto> onTermination)
	{
		/* resetting */
		spawner.ClearBeans ();
		gameTime.Reset ();

		/* initializing */
		this.player = player;
		this.controllerStates = new List<ControllerStateDto> ();
		this.onTermination = onTermination;

		/* activating */
		this.ui.Show ();
		this.enabled = true;
	}

	void Update ()
	{
		float leftTime = Mathf.Max (0f, gameDurationInSec - gameTime.time);
		bool gameTerminated = leftTime <= 0;

		if (gameTerminated) {

			onTermination (GameState.createDto(player, controllerStates,spawner.spawnedBeans));
			this.ui.Hide ();
			this.enabled = false;

		} else {
			ui.score = spawner.collectedBeanCount();
			ui.time = leftTime;
		}
	}

	void LateUpdate ()
	{
		if (gameTime.IsNewDocumentationFrame)
			controllerStates.Add (controller.State ());
	}
}
