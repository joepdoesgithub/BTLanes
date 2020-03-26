using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour{
	SPlayerOrder[] PlayerOrders;
	public SPlayerOrder[] GetPlayerOrders(){return PlayerOrders;}

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
		}else
		if(battleState == GEnums.EBattleState.EndPhase){
			BTEndPhaseHelper.DoEndPhase();
			Globals.SetBattleState(GEnums.EBattleState.MovingInit);
		}
    }

	void DoSelectNext(){
		bool done = true;

		for(int i = 0;i<PlayerOrders.Length;i++){
			if(PlayerOrders[i].hasActed == false){
				if(PlayerOrders[i].unit.IsUnitDestroyed()){
					PlayerOrders[i].hasActed = true;
					PlayerOrders[i].isActing = false;
					continue;
				}

				done = false;
				PlayerOrders[i].hasActed = true;
				PlayerOrders[i].isActing = true;

				GRefs.battleUnitManager.SelectMech(PlayerOrders[i].unit);

				if(PlayerOrders[i].unit.team != 0){
					//AI turn
					
					if(movingSelectNext){
						AIHelper helper = new AIHelper(PlayerOrders[i].ID);
						helper.DoAIMove();
						GRefs.battleUnitManager.FinishMove(PlayerOrders[i].unit);
					}else if(shootingSelectNext){
						Utils.ClearLogConsole();
						AIHelper helper = new AIHelper(PlayerOrders[i].ID);
						helper.DoAIShooting();
						GRefs.battleUnitManager.FinishShooting(PlayerOrders[i].unit);
					}else
						GRefs.battleUnitManager.FinishPhysical(PlayerOrders[i].unit);
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
				Globals.SetBattleState(GEnums.EBattleState.EndPhase);
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
		PlayerOrders = new SPlayerOrder[newList.Count];
		for(int i = 0;i<newList.Count;i++)
			PlayerOrders[i] = new SPlayerOrder(GLancesAndUnits.GetUnit(newList[i]));
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
		PlayerOrders = new SPlayerOrder[newList.Count];
		for(int i = 0;i<newList.Count;i++)
			PlayerOrders[i] = new SPlayerOrder(GLancesAndUnits.GetUnit(newList[i]));
	}

	public void FinishCurrentActingUnit(){
		for(int i = 0;i<PlayerOrders.Length;i++){
			if(PlayerOrders[i].isActing){
				PlayerOrders[i].isActing = false;
				return;
			}
		}
	}

	public bool HasUnitActed(int unitID){
		foreach(SPlayerOrder p in PlayerOrders){
			if(p.ID == unitID)
				return p.hasActed;
		}
		return false;
	}

	public struct SPlayerOrder{
		public Unit unit;
		public int ID;
		public bool hasActed;
		public bool isActing;
		public SPlayerOrder(Unit unit){this.unit = unit;ID = unit.ID;hasActed = false;isActing = false;}
	}
}
