using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour{
	SPlayerOrder[] PlayerOrder;
	public SPlayerOrder[] GetPlayerOrders(){return PlayerOrder;}

	GEnums.EBattleState battleState;

	void Start(){GRefs.battleManager = this;}

	bool movingSelectNext, shootingSelectNext, physicalSelectNext;

    // Update is called once per frame
    void Update(){
		battleState = Globals.GetBattleState();
		// Debug.Log(battleState.ToString());

		movingSelectNext = (battleState == GEnums.EBattleState.MovingSelectNext);
		shootingSelectNext = (battleState == GEnums.EBattleState.ShootingSelectNext);
		physicalSelectNext = (battleState == GEnums.EBattleState.PhysicalSelectNext);

		if(battleState == GEnums.EBattleState.AllInit)
			return;
        else if(battleState == GEnums.EBattleState.MovingInit){
			InitPhase(1);
			GRefs.battleUnitManager.NextPhase(battleState);
			Globals.SetBattleState(GEnums.EBattleState.MovingSelectNext);
		}else if(movingSelectNext || shootingSelectNext || physicalSelectNext)
			DoSelectNext();
		else 
		if(battleState == GEnums.EBattleState.ShootingInit){
			InitPhase(-1);
			Globals.SetBattleState(GEnums.EBattleState.ShootingSelectNext);
			GRefs.battleUnitManager.NextPhase(battleState);
		}else 
		if(battleState == GEnums.EBattleState.PhysicalInit){
			InitPhysicalPhase();
			Globals.SetBattleState(GEnums.EBattleState.PhysicalSelectNext);
			GRefs.battleUnitManager.NextPhase(battleState);
		}
    }

	void DoSelectNext(){
		bool done = true;

		for(int i = 0;i<PlayerOrder.Length;i++){
			if(PlayerOrder[i].hasActed == false){
				if(PlayerOrder[i].unit.IsUnitDestroyed()){
					PlayerOrder[i].hasActed = true;
					PlayerOrder[i].isActing = false;
					continue;
				}

				done = false;
				PlayerOrder[i].hasActed = true;
				PlayerOrder[i].isActing = true;

				GRefs.battleUnitManager.SelectMech(PlayerOrder[i].unit);

				if(PlayerOrder[i].unit.team != 0){
					//AI turn
					GlobalFuncs.PostMessage(string.Format("Doing AI {0} turn for " + PlayerOrder[i].unit.unitName, (movingSelectNext?"moving":"shooting") ));

					if(movingSelectNext)
						GRefs.battleUnitManager.FinishMove(PlayerOrder[i].unit);
					else if(shootingSelectNext)
						GRefs.battleUnitManager.FinishShooting(PlayerOrder[i].unit);
					else
						GRefs.battleUnitManager.FinishPhysical(PlayerOrder[i].unit);
					return;
				}
				if(movingSelectNext)
					Globals.SetBattleState(GEnums.EBattleState.MovingWaitingForInput);
				else if(shootingSelectNext)
					Globals.SetBattleState(GEnums.EBattleState.ShootingWaitingForInput);
				else if(physicalSelectNext)
					Globals.SetBattleState(GEnums.EBattleState.PhysicalWaitingForInput);
				return;
			}
		}

		if(done){
			GRefs.battleUnitManager.UnselectAllUnits();
			GRefs.btUnitDisplayManager.ResetSelections(true,true);
			if (movingSelectNext)
				Globals.SetBattleState(GEnums.EBattleState.ShootingInit);
			else if (shootingSelectNext)
				Globals.SetBattleState(GEnums.EBattleState.PhysicalInit);
			else if (physicalSelectNext)
				Globals.SetBattleState(GEnums.EBattleState.MovingInit);
		}
	}

	void InitPhysicalPhase(){
		List<int> newList = new List<int>();
		List<Unit> unitsInBattle = TacBattleData.GetAllUnitsInBattle();

		int maxPiloting = int.MinValue;
		foreach(Unit u in unitsInBattle)
			maxPiloting = (u.pilot.Pilotting > maxPiloting ? u.pilot.Pilotting : maxPiloting);

		for(int i = 0; i <= maxPiloting; i++){
			List<int> shortList = new List<int>();

			foreach(Unit u in unitsInBattle){
				if(u.pilot.Pilotting == i)
					shortList.Add(u.ID);
			}
			while(shortList.Count > 0){
				int index = UnityEngine.Random.Range(0,shortList.Count);
				newList.Add( shortList[index] );
				shortList.RemoveAt(index);
			}
		}
		PlayerOrder = new SPlayerOrder[newList.Count];
		for(int i = 0;i<newList.Count;i++)
			PlayerOrder[i] = new SPlayerOrder(GLancesAndUnits.GetUnit(newList[i]));
	}

	void InitPhase(int direction){
		List<int> newList = new List<int>();
		List<Unit> unitsInBattle = TacBattleData.GetAllUnitsInBattle();
		int startI = (direction>0?Globals.MinPilotInitiative:Globals.MaxPilotInitiative);
		int modder = (direction>0?1:-1);
		for(int i = startI; i <= Globals.MaxPilotInitiative && i >= Globals.MinPilotInitiative; i += modder){
			List<int> shortList = new List<int>();

			foreach(Unit u in unitsInBattle){
				if(u.pilot.Initiative == i)
					shortList.Add(u.ID);
			}
			while(shortList.Count > 0){
				int index = UnityEngine.Random.Range(0,shortList.Count);
				newList.Add( shortList[index] );
				shortList.RemoveAt(index);
			}
		}
		PlayerOrder = new SPlayerOrder[newList.Count];
		for(int i = 0;i<newList.Count;i++)
			PlayerOrder[i] = new SPlayerOrder(GLancesAndUnits.GetUnit(newList[i]));
	}

	public void FinishCurrentActingUnit(){
		for(int i = 0;i<PlayerOrder.Length;i++){
			if(PlayerOrder[i].isActing){
				PlayerOrder[i].isActing = false;
				return;
			}
		}
	}

	public struct SPlayerOrder{
		public Unit unit;
		public bool hasActed;
		public bool isActing;
		public SPlayerOrder(Unit unit){this.unit = unit;hasActed = false;isActing = false;}
	}
}
