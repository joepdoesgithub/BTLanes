using UnityEngine;

using UnityEditor;
using System.Reflection;
using System;

public static class GlobalFuncs{
	public static void PostMessage(string msg){
		if(Globals.screenState == GEnums.EScreenState.TacticalBattle && GRefs.tacBattConsole != null)
			GRefs.tacBattConsole.PostMessage(msg);
		else{
			Debug.LogError("GFuncs.PostMesage: output was null");
			Debug.Log(msg);
		}
	}
}


public static class Utils {
	static MethodInfo _clearConsoleMethod;
	static MethodInfo clearConsoleMethod {
		get {
			if (_clearConsoleMethod == null) {
				Assembly assembly = Assembly.GetAssembly (typeof(SceneView));
				Type logEntries = assembly.GetType ("UnityEditor.LogEntries");
				_clearConsoleMethod = logEntries.GetMethod ("Clear");
			}
			return _clearConsoleMethod;
		}
	}

	public static void ClearLogConsole() {
		clearConsoleMethod.Invoke (new object (), null);
	}
}