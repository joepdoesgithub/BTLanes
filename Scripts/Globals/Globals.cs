using UnityEngine;

public static class Globals{
	public static GEnums.EScreenState screenState;

	public static int MinPilotInitiative = 1;
	public static int MaxPilotInitiative = 10;
	
	static GEnums.EBattleState battleState = GEnums.EBattleState.AllInit;
	// public static bool BattleManagerInitDone = false;
	public static bool BattleUnitManagerInitDone = false;
	public static GEnums.EBattleState GetBattleState(){
		if(battleState == GEnums.EBattleState.AllInit && BattleUnitManagerInitDone)
			battleState = GEnums.EBattleState.MovingInit;
		return battleState;		
	}
	public static void SetBattleState(GEnums.EBattleState newState){
		if(battleState != GEnums.EBattleState.AllInit)
			battleState = newState;
	}

	public static Color32[,] UnitDisplayColors = {
		{ new Color32(34,250,0,255),new Color32(12,128,0,255) },
		{ new Color32(188,49,33,255),new Color32(109,13,3,255) } };
}