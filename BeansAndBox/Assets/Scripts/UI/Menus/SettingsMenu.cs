using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class SettingsMenu : AbstractMenu {

	public Level defaultLevel;
	private List<Level> levels;
	private Dropdown levelChooser;
		//savesDir = Path.Combine (Application.persistentDataPath, "Scores");

	public ProsthesisController defaultController;
	private List<ProsthesisController> controllers;
	private Dropdown controllerChooser;

	private MenuPanel panel;
	private MainMenu mainMenu;
	private Button cancelButton;
	private Button saveButton;
	private InputField controllerConfigDir;
	private InputField persistenceDir;
	private ProsthesisMover movementController;
	private Text errMsg;
	private SettingsDto dto;
	private SettingsDao dao;
	private Level currentLevel;
	private GameDataDao dataDao;

	protected override void _Awake() {
		/* logic objects */
		this.movementController = FindObjectOfType<ProsthesisMover> ();
		this.dao = FindObjectOfType<SettingsDao> ();
		this.dataDao = FindObjectOfType<GameDataDao> ();

		/* menu objects */
		this.panel = FindObjectOfType<MenuPanel> ();
		this.mainMenu = FindObjectOfType<MainMenu> ();

		/* ui elements */

		var buttons = new List<Button> (GetComponentsInChildren<Button> ());
		this.cancelButton = buttons.Find (b => b.name == "CancelButton");
		this.saveButton = buttons.Find (b => b.name == "SaveButton");

		var texts = new List<Text>(GetComponentsInChildren<Text> ());
		this.errMsg = texts.Find (x => x.name == "ErrMsg");

		var inputs = new List<InputField>(GetComponentsInChildren<InputField> ());
		this.controllerConfigDir = inputs.Find (x => x.name == "ConfigFile");
		this.persistenceDir = inputs.Find (x => x.name == "PersistenceDir");

		var choosers = new List<Dropdown> (GetComponentsInChildren<Dropdown> ());
		this.controllerChooser = choosers.Find (x => x.name == "ControllerChooser");
		this.levelChooser = choosers.Find (x => x.name == "LevelChooser");

		/* init choosers */
		this.controllers = new List<ProsthesisController>(FindObjectsOfType<ProsthesisController> ());

		foreach (var c in this.controllers) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = c.controllerName;
			this.controllerChooser.options.Add (d);
			c.enabled = false;
		}


		this.levels = new List<Level>(FindObjectsOfType<Level> ());
		foreach (var l in this.levels) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = l.name;
			this.levelChooser.options.Add (d);
			l.Disable ();
		}
		LoadSettingsDto (); 
		this.movementController.controller = controllers.Find (x => x.controllerName == dto.currentController);
		this.controllerChooser.value = controllerChooser.options.FindIndex (x => x.text == dto.currentController);
		this.levelChooser.value = levelChooser.options.FindIndex (x => x.text == dto.currentLevel);
		this.persistenceDir.text = dto.persistenceDir;
		LoadController ();
		LoadLevel ();
		LoadPersistenceDir ();
	}

	private void LoadSettingsDto() {
		var oldDto = dao.Load ();
		/* validating data */
		if (oldDto == null) {
			oldDto = new SettingsDto (
				currentLevel: defaultLevel.name,
				currentController: defaultController.controllerName, 
				controllerConfs: new ControllerConfigDto[0], 
				persistenceDir: Path.Combine(Application.persistentDataPath, "Scores"));
		}
		var confs = new ControllerConfigDto[controllers.Count];
		var oldCurrentCtrlValid = false;
		for (int i = 0; i < controllers.Count; i++) {
			var ctrl = controllers [i];
			var conf = oldDto.Find (ctrl.controllerName);
			if (conf == null) conf = new ControllerConfigDto (ctrl.controllerName, ctrl.defaultConfigFile);
			confs [i] = conf;
			oldCurrentCtrlValid = oldCurrentCtrlValid || oldDto.currentController == ctrl.controllerName;
		}

		this.dto = new SettingsDto (
			currentLevel: levels.Exists(l => l.name == oldDto.currentLevel) ? oldDto.currentLevel : defaultLevel.name,
			currentController: oldCurrentCtrlValid ? oldDto.currentController : defaultController.controllerName, 
			controllerConfs: confs,
			persistenceDir: oldDto.persistenceDir);
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
			controllerConfigDir.text = dto.Find(controller.controllerName).configuration;
			controllerConfigDir.readOnly = false;
		} else {
			controllerConfigDir.text = "( Does not need to be configured. )";
			controllerConfigDir.readOnly = true;
		}
	}

	public override void PanelClicked(){ }

	private void Cancel() {
		panel.Show (mainMenu);
	}	

	private bool LoadController() {
		var controller = controllers [this.controllerChooser.value];
		var result = controller.SetConfiguration (controllerConfigDir.text);
		if (result.isError) {
			errMsg.text = result.errMsg;
			return false;
		} else  {
			errMsg.text = "";
			movementController.controller.enabled = false;
			controller.enabled = true;
			movementController.controller = controller;

			var conf = dto.Find(controller.controllerName);
			conf.configuration = controllerConfigDir.text;
			dto.currentController = controller.controllerName;
			return true;
		}
	}

	private void LoadLevel () {
		if (this.currentLevel != null)
			this.currentLevel.Disable ();
		this.currentLevel = levels.Find (l => l.name == levelChooser.options [levelChooser.value].text);
		dto.currentLevel = currentLevel.name;
		currentLevel.Enable ();
	}

	private void LoadPersistenceDir() {
		dataDao.persistenceDir = persistenceDir.text;
		dto.persistenceDir = persistenceDir.text;
	}


	private void Save() {
		if (LoadController ()) {
			LoadLevel ();
			LoadPersistenceDir ();

			panel.Show (mainMenu);
			dao.Save (dto);		
		}
	}
}

