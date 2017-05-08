using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerLoader : MonoBehaviour {
	/// <summary> Loads a prosthesis controller or returns null if the controller was not loaded successfully. </summary>
	public abstract ProsthesisController Load();
}
