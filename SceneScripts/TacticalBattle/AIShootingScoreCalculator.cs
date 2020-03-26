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
		int toHit = unit.toHitModifier;
		GEnums.SWeapon[] wpns = unit.weapons;
		int maxHeatBuildup = GetMaxHeatBuildup();

		List<AIHelper.SShootingScores> dmgList = new List<AIHelper.SShootingScores>();

		// for each unit, check highest expected dmg and fire at that
		foreach(Unit u in TacBattleData.GetAllUnitsInBattle()){
			if(u.team != unit.team && (!u.IsUnitDestroyed())){
				int lane = GRefs.battleUnitManager.GetUnitLaneNum(u.ID);

				// Check if unit is in front arc
				if( (myFacing > 0 && myLane <= lane) || (myFacing < 0 && myLane >= lane) ){
					int enemyToBeHit = u.toBeHitModifier;
					int dist = Math.Abs(lane - myLane);
					int facing = GRefs.battleUnitManager.GetUnitFacing(u.ID);
					int terrainMod = 0;

					SWeaponExpDmg[] damages = GetSortedExpectedWeaponDmgs(wpns,unit.pilot.Gunnery,dist,toHit,enemyToBeHit,terrainMod);
					// foreach(SWeaponExpDmg swed in damages){
					// 	Debug.LogFormat("Back at GetShootingScores {0} {1} {2}", swed.weaponID, swed.expDmgPHeat, swed.heat);
					// }

					int heatRem = maxHeatBuildup;
					float dmgScore = 0f;
					List<int> wpnIDs = new List<int>();
					for(int i = 0;i<damages.Length;i++){
						if(heatRem <= 0)
							break;

						if(damages[i].heat <= heatRem + 4){
							dmgScore += damages[i].expDmgPHeat;
							wpnIDs.Add(damages[i].weaponID);
							heatRem -= damages[i].heat;
						}
					}

					AIHelper.SShootingScores score = new AIHelper.SShootingScores{
						enemyID = u.ID,
						score = dmgScore,
						weaponIDs = wpnIDs.ToArray()
					};
					dmgList.Add(score);
				}
			}
		}
		return dmgList.ToArray();
	}

	int GetMaxHeatBuildup(){
		return unit.heatSinking - unit.heat;
	}

	SWeaponExpDmg[] GetSortedExpectedWeaponDmgs(GEnums.SWeapon[] weapons, int gunnery, int dist, int toHit, int enemyToBeHit, int terrainMod){
		List<SWeaponExpDmg> tempList = new List<SWeaponExpDmg>();		
		foreach(GEnums.SWeapon wpn in weapons){
			SWeaponExpDmg o = new SWeaponExpDmg{
				weaponID = wpn.ID,
				expDmgPHeat = GetExpectedDmgPerHeat(wpn,dist,gunnery,toHit,enemyToBeHit,terrainMod),
				heat = (int)wpn.heat
			};
			tempList.Add(o);
		}

		// Sort them
		SWeaponExpDmg[] sorted = new SWeaponExpDmg[tempList.Count];
		int index = 0;
		while(tempList.Count > 0){
			int i = 0;
			float max = float.MinValue;
			for(int ii = 0;ii < tempList.Count;ii++){
				if(tempList[ii].expDmgPHeat > max){
					max = tempList[ii].expDmgPHeat;
					i = ii;
				}
			}
			sorted[index] = tempList[i];
			index++;
			tempList.RemoveAt(i);
		}

		return sorted;
	}

	float GetExpectedDmgPerHeat(GEnums.SWeapon wpn, int dist, int gunnery, int toHit, int enemyToBeHit, int terrainMod){
		int rangeMod = 0;
		if(wpn.ranges[0] > 0 && dist <= wpn.ranges[0])
			rangeMod = wpn.ranges[0] - dist + 1;
		else if(dist > wpn.ranges[1] && dist <= wpn.ranges[2])
			rangeMod = 2;
		else if(dist > wpn.ranges[2] && dist <= wpn.ranges[3])
			rangeMod = 4;
		else if(dist > wpn.ranges[3])
			rangeMod = 1000;
		int armmod = (wpn.loc == GEnums.EMechLocation.LA || wpn.loc == GEnums.EMechLocation.RA ? GGameStats.ArmMountedBonus : 0);
		int TN = gunnery + toHit + enemyToBeHit + rangeMod + terrainMod + armmod;

		float fraction = GetProb2D6( TN );

		// Debug.LogFormat("{0}:{1} ({2} * {3})/{4} = {5}",wpn.name, TN, fraction, wpn.GetDamage(), wpn.heat,		 (fraction * wpn.GetDamage())/wpn.heat );

		return (fraction * wpn.GetDamage())/wpn.heat;
	}

	static float TEMPGetShootingDMG(int[] wpnIDs){float dmg = 0;foreach(int i in wpnIDs){dmg += GLancesAndUnits.GetWeapon(i).GetDamage();}return dmg;}
	struct SMyS{public int id;public float damagedFactor;public int maxArmStruct;}
	public static int ShootingTargetSelection(List<AIHelper.SShootingScores> unitIDs){
		List<SMyS> list = new List<SMyS>();

		for(int i=0;i<unitIDs.Count;i++){
			int unitID = unitIDs[i].enemyID;
			Unit unit = GLancesAndUnits.GetUnit(unitID);
			int totalRem = unit.GetCurrentTotalArmour() + unit.GetCurrentTotalStruct();
			float totalMax = unit.armourMax + unit.structureMax;
			float damagedFactor = 0f;
			if(TEMPGetShootingDMG(unitIDs[i].weaponIDs) >= totalRem/2f)
				damagedFactor = 1f;
			else
				damagedFactor = 1 - totalRem/totalMax;

			SMyS s = new SMyS{
				id = unitIDs[i].enemyID,
				damagedFactor = damagedFactor,
				maxArmStruct = totalRem
			};
			list.Add(s);
		}

		List<SMyS> shortList = new List<SMyS>();
		float max = float.MinValue;
		for(int i = 0;i<list.Count;i++)
			max = (list[i].damagedFactor > max ? list[i].damagedFactor : max);
		for(int i = 0;i<list.Count;i++){
			if(list[i].damagedFactor == max)
				shortList.Add(list[i]);
		}
		if(shortList.Count == 1)
			return shortList[0].id;
		else if(shortList.Count < 1)
			Debug.LogError("HBNMTYUCVBY: Goes wrong!");
		
		int min = int.MaxValue;
		for(int i = 0;i<shortList.Count;i++)
			min = (shortList[i].maxArmStruct < min ? shortList[i].maxArmStruct : min);
		List<SMyS> shorterList = new List<SMyS>();
		for(int i = 0;i<shortList.Count;i++){
			if(shortList[i].maxArmStruct == min)
				shorterList.Add(shortList[i]);
		}
		return shorterList[ UnityEngine.Random.Range(0,shorterList.Count) ].id;
	}

	float GetProb2D6(int tn){
		int cnt = 0;
		for(int i = 1;i<=6;i++){
			for(int j =1;j<=6;j++){
				if(i+j >= tn)
					cnt++;
			}
		}
		return ((float)cnt)/36;
	}

	struct SWeaponExpDmg{
		public int weaponID;
		public float expDmgPHeat;
		public int heat;
	}
}