using UnityEngine;

public class BTMovementHelper{
	Unit unit;

	public BTMovementHelper(Unit unit){
		this.unit = unit;
	}

	public void TurnUnit(int turnDirection, int originalFacing, ref int moveRemaining, ref int runRemaining, out int newFacing){
		newFacing = originalFacing;
		if( (turnDirection < 0 && originalFacing < 0) || (turnDirection > 0 && originalFacing > 0) ){
			GlobalFuncs.PostMessage("Already facing that direction");
			return;
		}

		bool backwardMovement = GRefs.battleUnitManager.unitWalkedBackwards;
		if( (backwardMovement && moveRemaining <= 0 ) ||
					( (!backwardMovement) && runRemaining <= 0) ){
			GlobalFuncs.PostMessage("Not enough move left to turn");
			return;
		}

		moveRemaining--;
		runRemaining--;
		if(backwardMovement)
			runRemaining = 0;

		newFacing = (originalFacing<0?1:-1);
	}

	public void MoveUnit(int direction, int facing, ref int moveRemaining, ref int runRemaining, out int newLane){
		newLane = -1;

		// Find in which lane
		if(unit == null)
			return;

		int laneIndex = -1;
		foreach(BattleUnitManager.SUnitInLane u in GRefs.battleUnitManager.unitsInLanes){
			if(u.unit.ID == unit.ID){
				laneIndex = u.laneNum;
				break;
			}
		}

		// Check if found lane is in order and if movement is possible
		if(laneIndex < 0){
			Debug.LogError("BTMovementHelper.MoveUnit: Unit not found");
			return;
		}
		if(laneIndex == 0 && direction < 0){
			GlobalFuncs.PostMessage("Can't move further left");
			return;
		}else if(laneIndex == GRefs.battleUnitManager.GetLaneCount() - 1 && direction > 0){
			GlobalFuncs.PostMessage("Can't move further to the right");
			return;
		}

		// Check first available lane
		int laneAvailable = -1;
		int searchDir = (direction < 0 ? -1 : 1);
		for(int i = laneIndex + searchDir; i >= 0 && i < GRefs.battleUnitManager.GetLaneCount(); i += searchDir){
			if(GetNumUnitsInLane(i) < 2){
				laneAvailable = i;
				break;
			}
		}
		// Debug.Log("First available lane is: " + laneAvailable);
		if(laneAvailable < 0 && direction < 0){
			GlobalFuncs.PostMessage("Can't move further to the left");
			return;
		}
		if(laneAvailable < 0 && direction > 0){
			GlobalFuncs.PostMessage("Can't move further to the right");
			return;
		}

		// Check if movement enough
		int numSteps = Mathf.Abs(laneAvailable - laneIndex);

		if((direction > 0 && facing < 0) || (direction < 0 && facing > 0)){
			if(moveRemaining <= 0 || moveRemaining < numSteps){
				GlobalFuncs.PostMessage("Cant move backwards any further, not enough move remaining");
				return;
			}
			GRefs.battleUnitManager.unitWalkedBackwards = true;
		}else if( ( (!GRefs.battleUnitManager.unitWalkedBackwards) && runRemaining < numSteps) 
					|| (GRefs.battleUnitManager.unitWalkedBackwards && moveRemaining < numSteps)){
			GlobalFuncs.PostMessage("Not enough move available to reach next lane");
			return;
		}

		// s+= " Post: " + GRefs.battleUnitManager.unitWalkedBackwards.ToString();
		// Debug.Log(s);

		moveRemaining -= numSteps;
		runRemaining -= numSteps;
		if(GRefs.battleUnitManager.unitWalkedBackwards)
			runRemaining = 0;
		
		newLane = laneAvailable;
		GRefs.battleUnitManager.lanesMoved++;
	}

	public static int GetNumUnitsInLane(int lane){
		int cnt = 0;
		foreach(BattleUnitManager.SUnitInLane l in GRefs.battleUnitManager.unitsInLanes){
			if(l == null)
				continue;
			if(l.laneNum == lane)
				cnt++;
		}
		return cnt;
	}

	public static int GetToHitModifier(bool unitStationary, bool unitRan, bool unitJumped){
		if(unitStationary)
			return 0;
		if(unitRan)
			return 2;
		if(unitJumped)
			return 3;
		return 1;
	}

	public static int GetToBeHitModifier(bool unitJumped, int lanesMoved){
		int[,] table = {
			{0, 1, 0},
			{2, 2, 1},
			{3, 3, 2},
			{4, 5, 3},
			{6, 8, 4},
			{9, 12, 5},
			{13, int.MaxValue, 6} };
		int ret = (unitJumped? 1: 0);
		for(int i = 0;i<table.GetLength(0);i++){
			if(lanesMoved >= table[i,0] && lanesMoved <= table[i,1])
				return ret + table[i,2];
		}
		return ret;
	}
}