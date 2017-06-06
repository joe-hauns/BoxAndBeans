using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : AbstractMenu
{

	private GameLogic logic;
	private Button startButton;
	private Button tryButton;
	private Button exitButton;
	private Button settingsButton;
	private InputField playerName;
	private HighscoreView highScores;
	private MenuPanel menu;
	private GameDataDao dao;
	private TestRunLogic tryout;
	private SettingsMenu settings;

	// Use this for initialization
	override protected void _Awake()
	{
		this.menu = GameObject.FindObjectOfType<MenuPanel> ();
		this.highScores = GameObject.FindObjectOfType<HighscoreView> ();
		this.settings = FindObjectOfType<SettingsMenu> ();
		this.logic = GameObject.FindObjectOfType<GameLogic> ();
		this.playerName = GetComponentInChildren<InputField> ();
		this.dao = FindObjectOfType<GameDataDao> ();
		this.tryout = FindObjectOfType<TestRunLogic> ();
	
		var buttons = new List<Button> (GetComponentsInChildren<Button> ());
		this.startButton = buttons.Find (b => b.name == "StartButton");
		this.tryButton = buttons.Find (b => b.name == "TryButton");
		this.exitButton = buttons.Find (b => b.name == "ExitButton");
		this.settingsButton = buttons.Find (b => b.name == "SettingsButton");
	}

	void Start () {
		this.startButton.onClick.AddListener (() => LaunchGame ());
		this.tryButton.onClick.AddListener (() => TryGame());
		this.exitButton.onClick.AddListener (() => Exit() );
		this.settingsButton.onClick.AddListener (() => menu.Show(settings) );
	}

	void LaunchGame ()
	{
		var name = playerName.text;
		if (name != "") {
			menu.Hide ();
			logic.Launch (
				player: name, 
				onTermination: gameState => {
					dao.Save(gameState);
					menu.Show(highScores);
				});

		} else {
			print ("no name entered");
		}
	}

	void TryGame () 
	{
		menu.Hide ();
		tryout.Launch (
			onTermination: () => {
				menu.Show (this);
			}
		);
	}

	public override void KeyPressed (KeyCode keyCode)
	{
		//Event.current.keyCode
		switch (keyCode) {
		//case KeyCode.KeypadEnter: 
		//case KeyCode.Return:
			//LaunchGame ();
			//break;
		//case KeyCode.Escape:
			//Exit ();
			//break;
		default:
			break;
		}
	}

	private void Exit(){
		#if UNITY_EDITOR
		Debug.LogWarning("Quitting is not possible in editor mode.");
		#else
		Application.Quit();
		#endif
	}

	public override void PanelClicked () { }
}
