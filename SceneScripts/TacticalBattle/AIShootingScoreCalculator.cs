using System.Collections.Generic;
using UnityEngine;
using System;

class AIShootingScoreCalculator{
	int unitID;
	Unit unit;

	public AIShootingScoreCalculator(int unitID){
		this.unitID = unitID;
		unit = GLancesAndUnits.GetUnit(unitID);
	}

	// State based? About to die, don't care about heat
	public AIHelper.SShootingScores[] GetShootingScores(){
		int myLane = GRefs.battleUnitManager.GetUnitLaneNum(unitID);
		int myFacing = GRefs.battleUnitManager.GetUnitFacing(unitID);
		int myHeat = unit.heat;
		GEnums.SWeapon[] wpns = unit.weapons;

		// for each unit, check highest expected dmg and fire at that
		foreach(Unit u in TacBattleData.GetAllUnitsInBattle()){
			if(u.team != unit.team){
				int lane = GRefs.battleUnitManager.GetUnitLaneNum(u.ID);
				int dist = Math.Abs(lane - myLane);
				int facing = GRefs.battleUnitManager.GetUnitFacing(u.ID);

				// Check if unit is in front arc
				if( (myFacing > 0 && myLane <= lane) || (myFacing < 0 && myLane >= lane) ){
					
				}
			}
		}
	}
}