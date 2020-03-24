using System.Collections.Generic;
using UnityEngine;

static class AIHelper{
	// my own state (arm remaining etc)
	// defensive (hoeveel schade ik ga krijgen), hoe goed mijn modifier
	// offensive score
	// heat

	static bool first = true;

	static int unitID;

	static int maxLaneNum;
	
	public static void DoAIMove(int unitid){
		AIHelper.unitID = unitid;

		// Get list of possible moves
		List<SMove> moves = LoseSomeMoves( GetMoves() );
		// List<SLanePosition> evaluatedMoves = EvaluatePositions(moves);
		if(first){
			// first = false;
			string s = "";
			foreach(SMove m in moves)
				s += string.Format("{0} {1}, lanesMoves {2} running {3}\n",m.lane,m.facing,m.lanesMoved,m.running);
			Debug.Log(s);
		}
	}

	static List<SLanePosition> EvaluatePositions(List<SMove> moves){
		List<SLanePosition> sLanePositions = new List<SLanePosition>();
		foreach(SMove m in moves){
			bool stationary = (m.lanesMoved == 0);

			SLanePosition p = new SLanePosition{
				toHitMod = BTMovementHelper.GetToHitModifier( (m.lanesMoved==0), m.running, false),
				toBeHitMod = BTMovementHelper.GetToBeHitModifier(false, m.lanesMoved),
				smove = m
			};
			sLanePositions.Add(p);
		}

		AIScoreCalculator calc = new AIScoreCalculator(unitID);
		return calc.GetScoredPositions(sLanePositions);
	}

	static List<SMove> LoseSomeMoves(List<SMove> moves){
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

	static List<SMove> GetMoves(){
		maxLaneNum = GRefs.battleUnitManager.GetLaneCount() - 1;

		List<SMove> moves = new List<SMove>();

		int lane = GRefs.battleUnitManager.GetUnitLaneNum(unitID);
		int facing = GRefs.battleUnitManager.GetUnitLaneNum(unitID);
		int moveRemaining = GLancesAndUnits.GetUnit(unitID).walkSpeed;

		// Walking moves
		SMove currentPos = new SMove{
			lane = GRefs.battleUnitManager.GetUnitLaneNum(unitID),
			facing = GRefs.battleUnitManager.GetUnitfacing(unitID),
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

	static List<SMove> Deduplicate(List<SMove> Moves){
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

	static List<SMove> NextMoves(bool running, SMove position){
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