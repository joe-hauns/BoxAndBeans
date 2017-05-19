using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : AbstractMenu {

	private MenuPanel panel;
	private MainMenu mainMenu;
	private Button cancelButton;
	private Button saveButton;
	private Dropdown controllerChooser;
	private InputField configPath;
	private ProsthesisController[] controllers;
	private ProsthesisMovementController movementController;
	private Text errMsg;

	protected override void _Awake() {
		this.panel = FindObjectOfType<MenuPanel> ();
		this.mainMenu = FindObjectOfType<MainMenu> ();
		this.controllers = FindObjectsOfType<ProsthesisController> ();
		this.movementController = FindObjectOfType<ProsthesisMovementController> ();

		var buttons = new List<Button> (GetComponentsInChildren<Button> ());
		this.cancelButton = buttons.Find (b => b.name == "CancelButton");
		this.saveButton = buttons.Find (b => b.name == "SaveButton");

		var texts = new List<Text>(GetComponentsInChildren<Text> ());
		this.errMsg = texts.Find (x => x.name == "ErrMsg");
		this.configPath = GetComponentInChildren<InputField> ();
		this.controllerChooser = GetComponentInChildren<Dropdown> ();
	}

	void Start() {
		this.cancelButton.onClick.AddListener (() => Cancel ());
		this.saveButton.onClick.AddListener (() => Save ());
		foreach (var c in this.controllers) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = c.controllerName;
			//this.controllerChooser.options.Add (d);
		}
		foreach (var c in this.controllers) {
			Dropdown.OptionData d = new Dropdown.OptionData ();
			d.text = "lala";
			this.controllerChooser.options.Add (d);
		}

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
			configPath.text = controller.defaultConfigFile;
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
		}
	}
}
