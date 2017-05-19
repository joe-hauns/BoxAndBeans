using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SettingsDto {
	public string currentController;
	public SettingsDtoElem[] settings;

	public SettingsDto(string currentController, SettingsDtoElem[] settings) {
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
