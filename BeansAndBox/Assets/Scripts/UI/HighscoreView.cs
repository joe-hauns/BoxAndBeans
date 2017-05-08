using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;
using System.IO;
using System;
using UnityEngine.UI;

public class HighscoreView : AbstractMenu
{
	private ScoreList highscores;
	private ScoreList playerScores;
	private GameDataDao dao;
	private MenuPanel panel;
	private MainMenu mainMenu;
	private Text currentScore;

	override protected void _Awake ()
	{
		var scoreLists = new List<ScoreList> (GetComponentsInChildren<ScoreList> ());
		this.highscores = scoreLists.Find (lst => lst.name == "HighScores");
		this.playerScores = scoreLists.Find (lst => lst.name == "YourScores");
		this.dao = FindObjectOfType<GameDataDao> ();
		this.mainMenu = FindObjectOfType<MainMenu> ();
		this.panel = GetComponentInParent<MenuPanel> ();
		this.currentScore = new List<Text> (new List<RectTransform>(GetComponentsInChildren<RectTransform> ()).Find(r => r.name == "NBeans").GetComponentsInChildren<Text> ()).Find (t => t.gameObject.name == "Score");
	}

	void SetUpEntries ()
	{
		var allGames = this.dao.AllGames ();
		if (allGames.Count == 0) {
			Debug.LogAssertion ("This should never happen! It should always be the case that at least one game is saved before opening the Highscore view.");
			return;
		}
		var lastGame = allGames[0];
		foreach (var game in allGames) {
			if (game.dateTime.Ticks > lastGame.dateTime.Ticks)
				lastGame = game;
		}
		currentScore.text = ""+lastGame.score;

		{ /* player's scores */
			playerScores.SetEntries (
				entries: allGames.FindAll (dto => dto.participant.Equals(lastGame.participant)),
				titleConverter:  (GameStateDto dto) => dto.dateTime.ToString ("d.M.yyyy"));
		}
		{ /* highscore list */
			var entries = new Dictionary<string, GameStateDto> ();
			foreach (var next in allGames) {
				if (entries.ContainsKey (next.participant)) {
					var e = entries [next.participant];
					if (e.score < next.score)
						entries[next.participant] = next;
				} else {
					entries.Add (next.participant, next);
				}
			}
			var vals = new List<GameStateDto>(entries.Values);
			if (entries [lastGame.participant] != lastGame)
				vals.Add (lastGame);
				
			highscores.SetEntries(
				entries: vals,
				titleConverter: (GameStateDto dto) => dto.participant);
		}

	}

	public override bool shown {
		set {
			if (value) {
				SetUpEntries ();
			}			
			base.shown = value;
		}
	}

	public override void KeyPressed (KeyCode keyCode)
	{
		switch (keyCode) {
		case KeyCode.KeypadEnter: 
		case KeyCode.Escape:
		case KeyCode.Space:
		case KeyCode.Return:
			panel.Show (mainMenu);
			break;
		default:
			break;
		}
	}

	public override void PanelClicked ()
	{
		panel.Show (mainMenu);
	}
}