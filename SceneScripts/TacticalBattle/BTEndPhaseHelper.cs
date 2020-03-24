using System.Collections.Generic;

static class BTEndPhaseHelper{
	public static void DoEndPhase(){
		List<Unit> units = new List<Unit>();
		foreach(Unit u in GLancesAndUnits.units)
			units.Add(u);

		GlobalFuncs.PostMessage("   ");
		GlobalFuncs.PostMessage("End phase:");
		for(int team = 0;team <= 1;team++){
			for(int i = 0;i<units.Count;i++){
				if(units[i].team == team && (!units[i].IsUnitDestroyed()) ){
					// Do the cooling
					int sinking = units[i].heatSinking;
					sinking = (sinking > units[i].heat ? units[i].heat : sinking);
					int startH = units[i].heat;
					units[i].heat -= sinking;
					GlobalFuncs.PostMessage(string.Format("{0} sinks {1} heat from {2}, now at {3} heat",units[i].unitName,sinking,startH,units[i].heat));
					
					// Other stuff
				}
			}
		}
	}
}