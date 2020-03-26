using System.Collections.Generic;
using UnityEngine;
using System;

class AIScoreCalculator{
	int unitID;
	Unit unit;
	
	float[] idealWpnRange;
	float[] enemyDamageScores;
	int[] enemiesInLanes;
	
	public AIScoreCalculator(int unitID){
		this.unitID = unitID;
		unit = GLancesAndUnits.GetUnit(unitID);
	}

	public List<AIHelper.SLanePosition> GetScoredMovementPositions(List<AIHelper.SLanePosition> positions){
		idealWpnRange = GetIdealWpnRange(unitID);
		FillEnemiesInLanes();
		FillEnemyDamageScores();

		AIHelper.SLanePosition[] pos = positions.ToArray();

		for(int i = 0;i<pos.Length;i++){
			pos[i].scores = new float[3];
			pos[i].scores[0] = GetWeaponRangeScore(pos[i]);
			pos[i].scores[1] = GetDefensiveScore(pos[i]);
			pos[i].scores[2] = GetOffensiveScore(pos[i]);
		}

		return new List<AIHelper.SLanePosition>(pos);
	}

	//	For each lane, check the danger score
	//	Per unit, per weapon, per lane check dmg * chance of hitting (* iets als unit nog niet bewogen heeft)
	void FillEnemyDamageScores(){
		enemyDamageScores = new float[ GRefs.battleUnitManager.GetLaneCount() ];
		for(int i = 0;i<enemyDamageScores.Length;i++)
			enemyDamageScores[i] = 0f;

		foreach(Unit u in TacBattleData.GetAllUnitsInBattle()){
			if(u.team != unit.team && (!u.IsUnitDestroyed()) ){
				int enemyLane = GRefs.battleUnitManager.GetUnitLaneNum(u.ID);
				int enemyFacing = GRefs.battleUnitManager.GetUnitFacing(u.ID);
				bool enemyHasMoved = GRefs.battleManager.HasUnitActed(u.ID);

				foreach(GEnums.SWeapon wpn in u.weapons){
					for(int lane = 0; lane < GRefs.battleUnitManager.GetLaneCount(); lane++)
						enemyDamageScores[lane] += GetDmgPerUnitPerLane(lane, enemyLane,enemyFacing,wpn, enemyHasMoved);
				}
			}
		}

		float maxV = 0f;
		foreach(float f in enemyDamageScores)
			maxV = (f > maxV ? f : maxV);
		
		for(int i = 0;i<enemyDamageScores.Length;i++)
			enemyDamageScores[i] = 1f - enemyDamageScores[i] / maxV;


		// string s = "DMGs: ";
		// for(int i = 0;i<enemyDamageScores.Length;i++)
		// 	s += string.Format("|{0},{1}|, ",i,enemyDamageScores[i]);
		// Debug.Log(s);
	}

	float GetDmgPerUnitPerLane(int lane, int enemyUnitLane, int enemyUnitFacing, GEnums.SWeapon wpn, bool enemyHasMoved){
		if( (lane < enemyUnitLane && enemyUnitFacing > 0) ||
				(lane > enemyUnitLane && enemyUnitFacing < 0) )
			return 0f;
		float score = wpn.GetDamage();
		int dist = Math.Abs(enemyUnitLane - lane);
		if(wpn.ranges[0] > 0 && dist <= wpn.ranges[0])
			score *= (1f - 0.125f * (wpn.ranges[0] - dist + 1));
		else if(dist > wpn.ranges[1] && dist <= wpn.ranges[2])
			score *= 0.75f;
		else if(dist > wpn.ranges[2] && dist <= wpn.ranges[3])
			score *= 0.5f;
		else if(dist > wpn.ranges[3])
			score = 0f;

		score *= (enemyHasMoved ? 1f : 0.8f);
		return score;
	}

	float GetDefensiveScore(AIHelper.SLanePosition position){
		float enemyDmgAtLoc = enemyDamageScores[position.smove.lane];

		int spd = unit.runSpeed;
		float maxToBeHitMod = 0f;		
		for(int i = 0;i<GGameStats.ToBeHitLanesMoved.GetLength(0);i++){
			if(spd >= GGameStats.ToBeHitLanesMoved[i,0] && spd <= GGameStats.ToBeHitLanesMoved[i,1])
				maxToBeHitMod = (float)GGameStats.ToBeHitLanesMoved[i,2];
		}
		float toBeHitScore = 1f - (position.toBeHitMod / maxToBeHitMod);

		float inFront = 0, inBack = 0;
		foreach(Unit u in TacBattleData.GetAllUnitsInBattle()){
			if(u.team != unit.team && (!u.IsUnitDestroyed()) ){
				// int enemyFacing = GRefs.battleUnitManager.GetUnitFacing(u.ID);
				int enemyLane  = GRefs.battleUnitManager.GetUnitLaneNum(u.ID);
				int dist = Math.Abs( enemyLane - position.smove.lane);
				if( (position.smove.facing > 0 && enemyLane < position.smove.lane) ||
						(position.smove.facing < 0 && enemyLane > position.smove.lane) ){
					float score = 1f - 0.1f * (dist-1);
					score = (score < 0.1f ? 0.1f : score);
					inBack += score;
				}else{
					float score = 1f - 0.1f * (dist-1);
					score = (score < 0.1f ? 0.1f : score);
					inFront += score;
				}
			}			
		}
		float facingScore = (inFront >= inBack ? 1f : 0f);

		return (enemyDmgAtLoc + toBeHitScore + facingScore) / 3f;
	}

	float GetOffensiveScore(AIHelper.SLanePosition position){
		int maxRange = 0;
		foreach(GEnums.SWeapon wpn in unit.weapons)
			maxRange = (wpn.ranges[3] > maxRange ? wpn.ranges[3] : maxRange);
		
		// Debug.LogFormat("{0}, maxrange {1}",unit.unitName,maxRange);
		
		int currentEnemyDist = int.MaxValue;
		int newClosestEnemyDist = int.MaxValue;
		foreach(Unit u in TacBattleData.GetAllUnitsInBattle()){
			if(u.team != unit.team){
				int dist = Math.Abs(GRefs.battleUnitManager.GetUnitLaneNum(u.ID) - GRefs.battleUnitManager.GetUnitLaneNum(unitID));
				currentEnemyDist = (dist < currentEnemyDist ? dist : currentEnemyDist);
				// currentEnemyDist = (currentEnemyDist >= idealWpnRange.Length ? idealWpnRange.Length - 1 : currentEnemyDist);
				
				dist = Math.Abs(GRefs.battleUnitManager.GetUnitLaneNum(u.ID) - position.smove.lane);
				newClosestEnemyDist = (dist < newClosestEnemyDist ? dist : newClosestEnemyDist);
				// newClosestEnemyDist = (newClosestEnemyDist >= idealWpnRange.Length ? idealWpnRange.Length - 1 : newClosestEnemyDist);
			}
		}

		// This covers option (am already in range and going to move so that idealwpnrange decreases)
		float ret = 0f;
		// int option = 0;
		// out of range at both locations?
		if( currentEnemyDist > maxRange && newClosestEnemyDist > maxRange){
			if(newClosestEnemyDist > currentEnemyDist){
				// option = 1;
				ret = 0f;
			}else{
				// option = 2;
				ret = position.smove.lanesMoved / ((float)unit.runSpeed);
			}
		}else
		// can move into range
		if( currentEnemyDist > maxRange && newClosestEnemyDist <= maxRange ){
			// option = 3;
			ret = (1.1f + idealWpnRange[ newClosestEnemyDist ])/2f;
			ret = (ret > 1f ? 1f : ret);
		}else
		// moving out of range???
		if( currentEnemyDist <= maxRange && newClosestEnemyDist > maxRange){
			// option = 4;
			ret = 0f;
		}else
		// Already in range
		if( idealWpnRange[newClosestEnemyDist] > 
				idealWpnRange[currentEnemyDist]){
			// option = 5;
			ret = 1f;
		}
		// Debug.LogFormat("{0},{1},{2}: opt {3} score {4}",position.smove.lane,position.smove.facing,position.smove.lanesMoved,option,ret);
		
		return ret;
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

	float GetWeaponRangeScore(AIHelper.SLanePosition pos){
		int lane = pos.smove.lane;
		int facing = pos.smove.facing;
		// Start from current pos, if enemies check score
		// next tile, if enemies check score
		// return highest score in this way
		float dmgScore = 0f;

		for(int dist = 0; dist < idealWpnRange.Length; dist++){
			int newLane = lane + (facing > 0 ? 1 : -1) * dist;
			if(newLane >= 0 && newLane < enemiesInLanes.Length){
				if(enemiesInLanes[newLane] > 0 &&
						idealWpnRange[dist] > dmgScore)
					dmgScore = idealWpnRange[dist];
			}
		}

		float toHitScore = 1f - ( (float)pos.toHitMod / BTMovementHelper.GetMaxToHitModifier() );

		return (1.6f * dmgScore + 0.4f * toHitScore)/2f;
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
	
		// string s = GLancesAndUnits.GetUnit(unitID).unitName + " scores: ";
		for(int i = 0;i<rangeScores.Length;i++){
			rangeScores[i] = rangeScores[i] / maxVal;
			// s += string.Format("{0},{1} ",i,rangeScores[i]);
		}
		// Debug.Log(s);		

		return rangeScores;
	}
}