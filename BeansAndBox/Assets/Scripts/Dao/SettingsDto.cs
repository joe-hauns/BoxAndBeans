using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SettingsDto {
	public string currentLevel;
	public string currentController;
	public string persistenceDir;
	public SettingsDtoElem[] settings;

	public SettingsDto(string currentLevel,string currentController, SettingsDtoElem[] settings, string persistenceDir) {
		this.persistenceDir = persistenceDir;
		this.currentLevel = currentLevel;
		this.currentController = currentController;
		this.settings = settings;
	}

	public SettingsDtoElem Find(string controllerName) {
		foreach (var s in settings) {
			if (s.controllerName == controllerName)
				return s;
		}
		return null;
	}
}


[Serializable]
public class SettingsDtoElem {
	public string controllerName;
	public string configuration;

	public SettingsDtoElem(string controllerName, string configuration) {
		this.controllerName = controllerName;
		this.configuration = configuration;
	}
}
