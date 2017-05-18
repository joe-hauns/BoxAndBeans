using System;
using UnityEngine;

public abstract class ProsthesisController : MonoBehaviour {
	public class ConfigResult {
		public bool isError {get { return this != OK; }}
		public string errMsg;
		private ConfigResult(string errMsg) {
			this.errMsg = errMsg;
		}
		public static ConfigResult error(string msg) {
			return new ConfigResult(msg);
		}
		public static ConfigResult OK = new ConfigResult("");
	}

	public abstract string controllerName { get; }
	public abstract string defaultConfigFile { get; }
	public abstract bool needsConfiguration { get; }

	public abstract ConfigResult SetConfiguration (string configPath);

	/**
	 * @return a value within [-1; 1] where 
	 * 		 1 means the prostheses should open at full velocity,
	 * 		-1 means the prostheses should close at full velocity and
	 * 		 0 means the prostheses should neither open nor close
	 */
	public abstract float getOpeningVelocity();

	/**
	 * @return a value within [-1; 1] where
	 * 		-1 means turn counter clock wise at full velocity,
	 * 		 1 means turn clock wise at full velocity and
	 * 		 0 means stay still
	 */
	public abstract float getRotationVelocity();

	/** 
	 * @return position of the prosthesis within playing pane. Must be within [0; 1] for x & y
	 */
	public abstract Vector2 getPosition();

}
