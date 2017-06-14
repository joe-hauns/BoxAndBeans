using System;
using UnityEngine;

/// <summary>
/// This class must be implemented for adding a new controller to the game. 
/// The controller script must be attatched to any game object in the GameView scene to be loaded in the game.
/// Before the settings are saved <code>SetConfiguration</code> of the currently selected controller will be called with the currently entered configuration string. The return value of the result will determine whether the settings will be stored or not.
/// </summary>
public abstract class ProsthesisController : MonoBehaviour {
	/// <returns> a unique user-readable identifier for the controller class </returns>
	public abstract string controllerName { get; }

	/// <returns> path to the default the configuration file </returns>
	public abstract string defaultConfigFile { get; }

	/// <returns> wheter the controller needs to be configured or not </returns>
	public abstract bool needsConfiguration { get; }

	/// <returns> lets the controller load the given configuration file </returns>
	public abstract ConfigResult SetConfiguration (string configPath);

	/// <returns>
	/// a value within [-1; 1] where 
	///		 1 means the prostheses should open at full velocity,
	///		-1 means the prostheses should close at full velocity and
	///		 0 means the prostheses should neither open nor close
	/// </returns>
	public abstract float OpeningVelocity();

	/// <returns>
	/// a value within [-1; 1] where
	///		-1 means turn counter clock wise at full velocity,
	///		 1 means turn clock wise at full velocity and
	///		 0 means stay still
	/// </returns>
	public abstract float RotationVelocity();

	/// <returns>
	/// the position of the prosthesis within 2 dimensional game area. 
	/// 	both coordinates must be within [ -1; 1 ]
	/// </returns>
	public abstract Vector2 Position();
}

/// <summary>
/// The result of a call of <code>ProsthesisController.SetConfiguration<code>. 
/// Can be OK on success or an error message.
/// </summary>
public class ConfigResult {

	public bool isError { get { return this != OK; } }

	public string errMsg { get; private set; }

	/// <summary> 
	/// constructs a result representing failure with the given error message 
	/// </summary>
	public static ConfigResult error(string msg) {
		return new ConfigResult(msg);
	}

	/// <summary> represents success </summary>
	public static ConfigResult OK { get { return _OK; } }

	private static ConfigResult _OK = new ConfigResult("");

	private ConfigResult(string errMsg) {
		this.errMsg = errMsg;
	}
}
