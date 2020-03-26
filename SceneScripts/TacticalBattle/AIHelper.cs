using System.Collections.Generic;
using UnityEngine;

public class AIHelper{
	// my own state (arm remaining etc)
	// defensive (hoeveel schade ik ga krijgen), hoe goed mijn modifier
	// offensive score
	// heat

	int unitID;

	int maxLaneNum;

	public AIHelper(int unitID){
		this.unitID = unitID;
	}
	
	public void DoAIMove(){
		// Get list of possible moves
		List<SMove> moves = LoseSomeMoves( GetMoves() );
		// List<SMove> moves = GetMoves();
		List<SLanePosition> evaluatedMoves = EvaluateMovementsPositions(moves);

		float maxV = 0f;
		int ii = 0;
		float[] weights = GetMovementWeightsDependentOnUnitState();
		float wSum = 0f;
		foreach(float f in weights)
			wSum += f;
		if(weights.Length != evaluatedMoves[0].scores.Length || wSum != 3f)
			Debug.LogErrorFormat("WeightSum is {0}, lengths is {2}",wSum,weights.Length);
		string s = GLancesAndUnits.GetUnit(unitID).unitName + "\n";
		for(int i = 0;i<evaluatedMoves.Count;i++){
			float avgScore = 0f;
			for(int j = 0;j<evaluatedMoves[i].scores.Length;j++)
				avgScore += weights[j] * evaluatedMoves[i].scores[j];

			if(avgScore > maxV){
				maxV = avgScore;
				ii = i;
			}

			s += string.Format("|{0}{1},  {2}, {3},    {4}, {5},    {6}, {7} [{8}]|\n",
					evaluatedMoves[i].smove.lane,
					evaluatedMoves[i].smove.facing,
					evaluatedMoves[i].scores[0], weights[0],
					evaluatedMoves[i].scores[1], weights[1],
					evaluatedMoves[i].scores[2], weights[2],
					avgScore);
		}
		// Debug.Log(s);
		// Debug.LogFormat("{0}: best is {1},{2}",GLancesAndUnits.GetUnit(unitid).unitName,
		// 			evaluatedMoves[ii].smove.lane,
		// 			evaluatedMoves[ii].smove.facing);

		GRefs.battleUnitManager.MoveAIUnit(unitID,evaluatedMoves[ii]);
	}

	float[] GetMovementWeightsDependentOnUnitState(){
		Unit u = GLancesAndUnits.GetUnit(unitID);
		int arm = u.GetCurrentTotalArmour();
		int structure = u.GetCurrentTotalStruct();
		int maxArm = u.armourMax;
		int maxStruct = u.structureMax;
		bool partDestroyed = u.IsAnyPartDestroyed();

		if( (arm + structure)/((float)maxArm + maxStruct) >= 0.85f)
			return new float[]{1.2f,0.2f,1.6f};
		else if( (!partDestroyed) && (arm + structure)/((float)maxArm + maxStruct) >= 0.75f)
			return new float[]{1.3f,0.4f,1.3f};
		else if( (arm + structure)/((float)maxArm + maxStruct) < 0.5f || partDestroyed )
			return new float[]{0.5f,2f,0.5f};
		else
			return new float[]{1.05f,0.9f,1.05f};
	}

	List<SLanePosition> EvaluateMovementsPositions(List<SMove> moves){
		List<SLanePosition> sLanePositions = new List<SLanePosition>();
		foreach(SMove m in moves){
			SLanePosition p = new SLanePosition{
				toHitMod = BTMovementHelper.GetToHitModifier( (m.lanesMoved==0), m.running, false),
				toBeHitMod = BTMovementHelper.GetToBeHitModifier(false, m.lanesMoved),
				smove = m
			};
			sLanePositions.Add(p);
		}

		AIMovementScoreCalculator calc = new AIMovementScoreCalculator(unitID);
		return calc.GetScoredMovementPositions(sLanePositions);
	}

	List<SMove> LoseSomeMoves(List<SMove> moves){
		if(moves.Count == 1)
			return moves;

		List<SMove> newMoves = new List<SMove>();
		foreach(SMove m in moves)
			newMoves.Add(m);

		int numToLose = (int)Mathf.Round( moves.Count * GGameStats.MovesLostFraction);
		numToLose = (numToLose <= 0 ? 1 : numToLose);
		while(moves.Count - numToLose < 1 && numToLose > 0)
			numToLose--;
		
		for(int i = 0;i<numToLose;i++){
			int ii = UnityEngine.Random.Range(0,newMoves.Count);
			newMoves.RemoveAt(ii);
		}
		return newMoves;
	}

	////////////////
	//
	//		Get all the moves possible
	//
	////////////////

	List<SMove> GetMoves(){
		maxLaneNum = GRefs.battleUnitManager.GetLaneCount() - 1;

		List<SMove> moves = new List<SMove>();

		int lane = GRefs.battleUnitManager.GetUnitLaneNum(unitID);
		int facing = GRefs.battleUnitManager.GetUnitLaneNum(unitID);
		int moveRemaining = GLancesAndUnits.GetUnit(unitID).walkSpeed;

		// Walking moves
		SMove currentPos = new SMove{
			lane = GRefs.battleUnitManager.GetUnitLaneNum(unitID),
			facing = GRefs.battleUnitManager.GetUnitFacing(unitID),
			lanesMoved = 0,
			moveRemaining = GLancesAndUnits.GetUnit(unitID).walkSpeed,
			running = false
		};
		moves.AddRange( NextMoves(false, currentPos) );

		// Running moves
		currentPos.moveRemaining = GLancesAndUnits.GetUnit(unitID).runSpeed;
		currentPos.running = true;
		moves.AddRange( NextMoves(true, currentPos) );

		// Check if position is possible
		for(int i = 0;i<moves.Count;i++){
			if( BTMovementHelper.GetNumUnitsInLane( moves[i].lane ) >= 2){
				moves.RemoveAt(i);
				i = 0;
			}
		}

		// Deduplicate
		moves = Deduplicate(moves);	
		
		return moves;
	}

	List<SMove> Deduplicate(List<SMove> Moves){
		if(Moves.Count <= 1)
			return Moves;

		List<SMove> moves = new List<SMove>();
		foreach(SMove m in Moves)
			moves.Add(m);

		int removeIndex;
		for(int i = 0;i<moves.Count - 1;i++){
			for(int j = i+1; j<moves.Count;j++){
				if(moves[j].lane == moves[i].lane &&
						moves[j].facing == moves[i].facing &&
						moves[j].lanesMoved == moves[i].lanesMoved){
					if(moves[i].running != moves[j].running)
						removeIndex = (moves[i].running ? i : j);
					else
						removeIndex = j;
					
					moves.RemoveAt(removeIndex);
					i = 0;
					break;
				}
			}
		}
		
		return moves;
	}

	List<SMove> NextMoves(bool running, SMove position){
		List<SMove> moves = new List<SMove>();
		if(position.moveRemaining <= 0){
			moves.Add(position);
			return moves;
		}

		// Walk forwards
		if( !((position.lane == 0 && position.facing < 0) || (position.lane == maxLaneNum && position.facing > 0)) ){
			SMove forward = new SMove{
				lane = position.lane + (position.facing < 0 ? -1 : 1),
				facing = position.facing,
				lanesMoved = position.lanesMoved + 1,
				moveRemaining = position.moveRemaining -1,
				running = position.running
			};
			moves.AddRange(NextMoves(running, forward));
		}

		// Turn around
		SMove turn = new SMove{
			lane = position.lane,
			facing = -1 * position.facing,
			lanesMoved = position.lanesMoved,
			moveRemaining = position.moveRemaining - 1,
			running = position.running
		};
		moves.AddRange(NextMoves(running,turn));

		// Walking backwards
		if(!running){
			if( !((position.lane == 0 && position.facing > 0) || (position.lane == maxLaneNum && position.facing < 0)) ){
				SMove backwards = new SMove{
					lane = position.lane + (position.facing > 0 ? -1 : 1),
					facing = position.facing,
					lanesMoved = position.lanesMoved + 1,
					moveRemaining = position.moveRemaining - 1,
					running = false
				};
				moves.AddRange(NextMoves(running,backwards));
			}
		}

		// Standing still
		SMove standStill = new SMove{
			lane = position.lane,
			facing = position.facing,
			lanesMoved = position.lanesMoved,
			moveRemaining = position.moveRemaining - 1,
			running = position.running
		};
		moves.AddRange(NextMoves(running,standStill));

		return Deduplicate(moves);
	}

	////////////////
	//
	//		Shooting
	//
	////////////////
	public void DoAIShooting(){
		AIShootingScoreCalculator helper = new AIShootingScoreCalculator(unitID);
		SShootingScores[] scores = helper.GetShootingScores();
	}

	public struct SShootingScores{
		public int enemyID;
		public float[] scores;
		public int[] weaponIDs;
	}

	public struct SLanePosition{
		public int toHitMod;
		public int toBeHitMod;

		public SMove smove;

		public float[] scores;
	}

	public struct SMove{
		public int lane;
		public int facing;
		public int lanesMoved;
		public int moveRemaining;
		public bool running;
	}
}