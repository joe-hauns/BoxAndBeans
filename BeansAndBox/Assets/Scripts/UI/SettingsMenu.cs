using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsMenu : AbstractMenu {

	public Level defaultLevel;
	private List<Level> levels;
	private Dropdown levelChooser;

	public ProsthesisController defaultController;
	private List<ProsthesisController> controllers;
	private Dropdown controllerChooser;

	private MenuPanel panel;
	private MainMenu mainMenu;
	private Button cancelButton;
	private Button saveButton;
	private InputField configPath;
	private ProsthesisMovementController movementController;
	private Text errMsg;
	private SettingsDto dto;
	private SettingsDao dao;
	private Level currentLevel;

	protected override void _Awake() {
		/* logic objects */
		this.movementController = FindObjectOfType<ProsthesisMovementController> ();
		this.dao = FindObjectOfType<SettingsDao> ();


		/* menu objects */
		this.panel = FindObjectOfType<MenuPanel> ();
		this.mainMenu = FindObjectOfType<MainMenu> ();

		/* ui elements */

		var buttons = new List<Button> (GetComponentsInChildren<Button> ());
		this.cancelButton = buttons.Find (b => b.name == "CancelButton");
		this.saveButton = buttons.Find (b => b.name == "SaveButton");

		var texts = new List<Text>(GetComponentsInChildren<Text> ());
		this.errMsg = texts.Find (x => x.name == "ErrMsg");
		this.configPath = GetComponentInChildren<InputField> ();

		var choosers = new List<Dropdown> (GetComponentsInChildren<Dropdown> ());
		this.controllerChooser = choosers.Find (x => x.name == "ControllerChooser");
		this.levelChooser = choosers.Find (x => x.name == "LevelChooser");

		/* init choosers */
		this.controllers = new List<ProsthesisController>(FindObjectsOfType<ProsthesisController> ());

		foreach (var c in this.controllers) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = c.controllerName;
			this.controllerChooser.options.Add (d);
		}


		this.levels = new List<Level>(FindObjectsOfType<Level> ());
		foreach (var l in this.levels) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = l.levelName;
			this.levelChooser.options.Add (d);
			l.Disable ();
		}

		this.dto = dao.Load ();
		validateDto (); // may be invalid if saved by an older version of the game
		{
			this.movementController.controller = controllers.Find (x => x.controllerName == dto.currentController);
			this.controllerChooser.value = controllerChooser.options.FindIndex (x => x.text == dto.currentController);
		}
		LoadLevel ();
	}

	private void validateDto() {
		/* checking if data for each controller is available or setting to default if not */
		if (dto == null) {
			dto = new SettingsDto (defaultLevel.levelName,defaultController.controllerName, new SettingsDtoElem[0]);
		}
		var oldDto = dto;
		var confs = new SettingsDtoElem[controllers.Count];
		var oldCurrentCtrlValid = false;
		for (int i = 0; i < controllers.Count; i++) {
			var ctrl = controllers [i];
			var conf = oldDto.Find (ctrl.controllerName);
			if (conf == null) conf = new SettingsDtoElem (ctrl.controllerName, ctrl.defaultConfigFile);
			confs [i] = conf;
			oldCurrentCtrlValid = oldCurrentCtrlValid || oldDto.currentController == ctrl.controllerName;
		}

		this.dto = new SettingsDto (
			currentLevel: levels.Exists(l => l.levelName == dto.currentLevel) ? dto.currentLevel : defaultLevel.levelName,
			currentController: oldCurrentCtrlValid ? oldDto.currentController : defaultController.controllerName, 
			settings: confs);
	}

	void Start() {
		this.cancelButton.onClick.AddListener (() => Cancel ());
		this.saveButton.onClick.AddListener (() => Save ());

		controllerChooser.RefreshShownValue ();
		levelChooser.RefreshShownValue ();
		ControllerChooserValueChanged ();
	}

	public override void KeyPressed(KeyCode keyCode) {
		switch (keyCode) {
		case KeyCode.Escape:
			Cancel ();
			break;
		/*case KeyCode.KeypadEnter: 
		case KeyCode.Return:
			Save ();
			break;*/
		default:
			break;
		}
	}

	public void ControllerChooserValueChanged() {
		errMsg.text = "";
		var controller = controllers [this.controllerChooser.value];
		if (controller.needsConfiguration) {
			configPath.text = dto.Find(controller.controllerName).configuration;
			configPath.readOnly = false;
		} else {
			configPath.text = "( Does not need to be configured. )";
			configPath.readOnly = true;
		}
	}

	public override void PanelClicked(){ }

	private void Cancel() {
		panel.Show (mainMenu);
	}	

	private bool LoadController() {
		var controller = controllers [this.controllerChooser.value];
		var result = controller.SetConfiguration (configPath.text);
		if (result.isError) {
			errMsg.text = result.errMsg;
			return false;
		} else  {
			errMsg.text = "";
			movementController.controller.enabled = false;
			controller.enabled = true;
			movementController.controller = controller;

			var conf = dto.Find(controller.controllerName);
			conf.configuration = configPath.text;
			dto.currentController = controller.controllerName;
			return true;
		}
	}

	private void LoadLevel () {
		if (this.currentLevel != null)
			this.currentLevel.Disable ();
		this.currentLevel = levels.Find (l => l.levelName == levelChooser.options [levelChooser.value].text);
		dto.currentLevel = currentLevel.levelName;
		currentLevel.Enable ();
	}


	private void Save() {
		if (LoadController ()) {
			panel.Show (mainMenu);
			LoadLevel ();
			dao.Save (dto);		
		}
	}
}

