using System.Collections.Generic;

public static class TacBattleData{
	public static Lance[] lances;
	public static Map.MapTerrain terrain = Map.MapTerrain.NULL;

	public static void EndBattle(){
		lances = null;
		terrain = Map.MapTerrain.NULL;
	}

	public static List<Unit> GetAllUnitsInBattle(){
		List<Unit> ret = new List<Unit>();
		foreach(Lance l in lances){
			foreach(Unit u in l.units)
				ret.Add(u);
		}
		return ret;
	}

	public static void InitiateBattleData(Map.MapTerrain mapTerrain, Lance[] battleLances){
		EndBattle();
		terrain = mapTerrain;
		lances = new Lance[2];
		lances[0] = battleLances[0];
		lances[1] = battleLances[1];
	}
}