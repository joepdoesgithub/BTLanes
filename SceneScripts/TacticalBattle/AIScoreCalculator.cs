using System.Collections.Generic;
using UnityEngine;
using System;

class AIScoreCalculator{
	int unitID;
	Unit unit;
	
	float[] idealWpnRange;
	int[] enemiesInLanes;
	
	public AIScoreCalculator(int unitID){
		this.unitID = unitID;
		unit = GLancesAndUnits.GetUnit(unitID);
	}

	public List<AIHelper.SLanePosition> GetScoredPositions(List<AIHelper.SLanePosition> positions){
		idealWpnRange = GetIdealWpnRange(unitID);
		FillEnemiesInLanes();

		AIHelper.SLanePosition[] pos = positions.ToArray();

		// string s = "";
		for(int i = 0;i<pos.Length;i++){
			pos[i].scores = new float[1];
			pos[i].scores[0] = GetWeaponRangeScore( pos[i].smove.lane, pos[i].smove.facing );
		}

		return new List<AIHelper.SLanePosition>(pos);
	}

	void FillEnemiesInLanes(){
		enemiesInLanes = new int[ GRefs.battleUnitManager.GetLaneCount() ];
		for(int i = 0;i<enemiesInLanes.Length;i++)
			enemiesInLanes[i] = 0;
		
		foreach(Unit u in TacBattleData.GetAllUnitsInBattle()){
			if(u.team != unit.team)
				enemiesInLanes[ GRefs.battleUnitManager.GetUnitLaneNum(u.ID) ]++;
		}
	}

	float GetWeaponRangeScore(int lane, int facing){
		// Start from current pos, if enemies check score
		// next tile, if enemies check score
		// return highest score in this way
		float score = float.MinValue;

		for(int dist = 0; dist < idealWpnRange.Length; dist++){
			int newLane = lane + (facing > 0 ? 1 : -1) * dist;
			if(newLane >= 0 && newLane < enemiesInLanes.Length){
				if(enemiesInLanes[newLane] > 0 &&
						idealWpnRange[dist] > score)
					score = idealWpnRange[dist];
			}
		}
		return score;
	}

	// f( range, heat, dmg, loc)
	// Per range, per weapon
	//		heat + wpnHeat <= sinking ? 1 : 0.5f
	//		range (>=+4,.2),(+2,.6),(+0,1),
	//		normalize to maxDmg
	float[] GetIdealWpnRange(int unitID){
		GEnums.SWeapon[] wpns = GLancesAndUnits.GetUnit(unitID).weapons;
		int heat = GLancesAndUnits.GetUnit(unitID).heat;
		int heatSinking = GLancesAndUnits.GetUnit(unitID).heatSinking;

		float maxDmg = 0;
		int maxRange = 0;
		foreach(GEnums.SWeapon wpn in wpns){
			maxDmg = (wpn.GetDamage() > maxDmg ? wpn.GetDamage() : maxDmg);
			maxRange = (wpn.ranges[3] > maxRange ? wpn.ranges[3] : maxRange);
		}

		float[] rangeScores = new float[GGameStats.AllWeaponsMaxRange + 1];
		for(int i = 0;i<rangeScores.Length;i++)
			rangeScores[i] = 0f;

		foreach(GEnums.SWeapon wpn in wpns){
			float heatScore = (heat + wpn.heat > heatSinking ? 0.5f : 1f);
			float dmgScore = wpn.GetDamage() / maxDmg;
			float armScore = (wpn.IsArmMounted() ? 1f : 0.85f);

			for(int rng = 0;rng < rangeScores.Length;rng++){
				float rngScore = 0f;
				if(wpn.ranges[0] > 0 && rng <= wpn.ranges[0]){
					rngScore = 1f - (wpn.ranges[0] - rng) * 0.2f;
					rngScore = (rngScore < 0f ? 0f : rngScore);
				}else if(rng <= wpn.ranges[1])
					rngScore = 1f;
				else if(rng > wpn.ranges[1] && rng <= wpn.ranges[2])
					rngScore = 0.6f;
				else if(rng > wpn.ranges[2] && rng <= wpn.ranges[3])
					rngScore = 0.2f;

				float score = (heatScore + dmgScore + rngScore + armScore) / 4f;
				score = (rngScore <= 0f ? 0f : rngScore);
				rangeScores[rng] += score;
			}
		}

		float maxVal = 0;
		for(int i = 0;i<rangeScores.Length;i++)
			maxVal = (rangeScores[i] > maxVal ? rangeScores[i] : maxVal);		
	
		for(int i = 0;i<rangeScores.Length;i++)
			rangeScores[i] = rangeScores[i] / maxVal;

		return rangeScores;
	}
}