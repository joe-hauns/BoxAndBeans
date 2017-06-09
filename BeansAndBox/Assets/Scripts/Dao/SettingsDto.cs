using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SettingsDto {
	public string currentLevel;
	public string currentController;
	public string persistenceDir;
	public ControllerConfigDto[] controllerConfs;

	public SettingsDto(string currentLevel,string currentController, ControllerConfigDto[] controllerConfs, string persistenceDir) {
		this.persistenceDir = persistenceDir;
		this.currentLevel = currentLevel;
		this.currentController = currentController;
		this.controllerConfs = controllerConfs;
	}

	public ControllerConfigDto Find(string controllerName) {
		foreach (var s in controllerConfs) {
			if (s.controllerName == controllerName)
				return s;
		}
		return null;
	}
}


[Serializable]
public class ControllerConfigDto {
	public string controllerName;
	public string configuration;

	public ControllerConfigDto(string controllerName, string configuration) {
		this.controllerName = controllerName;
		this.configuration = configuration;
	}
}
