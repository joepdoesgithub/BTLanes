using UnityEngine;

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