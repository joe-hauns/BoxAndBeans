using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : AbstractMenu {

	public ProsthesisController defaultController;

	private MenuPanel panel;
	private MainMenu mainMenu;
	private Button cancelButton;
	private Button saveButton;
	private Dropdown controllerChooser;
	private InputField configPath;
	private ProsthesisController[] controllers;
	private ProsthesisMovementController movementController;
	private Text errMsg;
	private SettingsDto dto;
	private SettingsDao dao;

	protected override void _Awake() {
		this.panel = FindObjectOfType<MenuPanel> ();
		this.mainMenu = FindObjectOfType<MainMenu> ();
		this.controllers = FindObjectsOfType<ProsthesisController> ();
		this.movementController = FindObjectOfType<ProsthesisMovementController> ();
		this.dao = FindObjectOfType<SettingsDao> ();

		var buttons = new List<Button> (GetComponentsInChildren<Button> ());
		this.cancelButton = buttons.Find (b => b.name == "CancelButton");
		this.saveButton = buttons.Find (b => b.name == "SaveButton");

		var texts = new List<Text>(GetComponentsInChildren<Text> ());
		this.errMsg = texts.Find (x => x.name == "ErrMsg");
		this.configPath = GetComponentInChildren<InputField> ();
		this.controllerChooser = GetComponentInChildren<Dropdown> ();


		foreach (var c in this.controllers) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = c.controllerName;
			this.controllerChooser.options.Add (d);
		}

		this.dto = dao.Load ();
		{
			/* checking if data for each controller is available or setting to default if not */
			if (dto == null) {
				dto = new SettingsDto (defaultController.controllerName, new SettingsDtoElem[0]);
			}
			var oldDto = dto;
			var confs = new SettingsDtoElem[controllers.Length];
			var oldCurrentValid = false;
			for (int i = 0; i < controllers.Length; i++) {
				var ctrl = controllers [i];
				var conf = oldDto.Find (ctrl.controllerName);
				if (conf == null) conf = new SettingsDtoElem (ctrl.controllerName, ctrl.defaultConfigFile);
				confs [i] = conf;
				oldCurrentValid = oldCurrentValid || oldDto.currentController == ctrl.controllerName;
			}
			this.dto = new SettingsDto (oldCurrentValid ? oldDto.currentController : defaultController.controllerName, confs);
		}

		{
			var controllers = new List<ProsthesisController> (this.controllers);
			this.movementController.controller = controllers.Find (x => x.controllerName == dto.currentController);
			this.controllerChooser.value = controllerChooser.options.FindIndex (x => x.text == dto.currentController);
		}

	}

	void Start() {
		this.cancelButton.onClick.AddListener (() => Cancel ());
		this.saveButton.onClick.AddListener (() => Save ());

		controllerChooser.RefreshShownValue ();
		ChooserValueChanged ();
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

	public void ChooserValueChanged() {
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

	public override void PanelClicked(){
		
	}

	private void Cancel() {
		panel.Show (mainMenu);
	}	

	private void Save() {
		var controller = controllers [this.controllerChooser.value];
		var result = controller.SetConfiguration (configPath.text);
		if (result.isError) {
			errMsg.text = result.errMsg;
		} else  {
			errMsg.text = "";
			panel.Show (mainMenu);
			movementController.controller.enabled = false;
			controller.enabled = true;
			movementController.controller = controller;

			var conf = dto.Find(controller.controllerName);
			conf.configuration = configPath.text;
			dto.currentController = controller.controllerName;
			dao.Save (dto);		
		}
	}
}
