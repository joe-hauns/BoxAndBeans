using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour {
	private AbstractMenu current;
	private Canvas canvas;
	private AbstractMenu[] menus;

	void Awake () {
		this.current = GetComponentInChildren<MainMenu> ();
		this.canvas = GetComponent<Canvas> ();
		this.menus = GetComponentsInChildren<AbstractMenu> ();
	}

	// Use this for initialization
	void Start () {
		foreach (var menu in menus) {
			menu.shown = false;
		}
		current.shown = true;
	}

	public void OnClicked () {
		this.current.PanelClicked ();
	}

	void OnGUI () {
		if (Event.current.type == EventType.KeyUp){
			this.current.KeyPressed (Event.current.keyCode);
		}
	}

	public void Show (AbstractMenu menu)
	{
		current.shown =  false;
		current = menu;
		current.shown =  true;
		this.canvas.enabled = true;
		this.enabled = true;
	}


	public void Hide ()
	{
		current.shown = false;
		this.canvas.enabled = false;
		this.enabled = false;
	}
}
