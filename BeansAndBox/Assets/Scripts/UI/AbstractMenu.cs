using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMenu : MonoBehaviour {

	public virtual bool shown { set { this.gameObject.SetActive (value); } }

	void Awake () {
		_Awake ();
	}

	protected abstract void _Awake ();
	public abstract void KeyPressed(KeyCode keyCode);
	public abstract void PanelClicked();
}
