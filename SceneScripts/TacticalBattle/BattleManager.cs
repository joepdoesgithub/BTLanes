using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour{
	SPlayerOrder[] PlayerOrder;
	public SPlayerOrder[] GetPlayerOrders(){return PlayerOrder;}

	GEnums.EBattleState battleState;

	void Start(){GRefs.battleManager = this;}

    // Update is called once per frame
    void Update(){
		battleState = Globals.GetBattleState();

		if(battleState == GEnums.EBattleState.AllInit)
			return;
        else if(battleState == GEnums.EBattleState.MovingInit){
			InitPhase(1);
			Globals.SetBattleState(GEnums.EBattleState.MovingSelectNext);
			GRefs.battleUnitManager.NextPhase();
		}else if(battleState == GEnums.EBattleState.MovingSelectNext ||
					battleState == GEnums.EBattleState.ShootingSelectNext){
			bool done = true;
			for(int i = 0;i<PlayerOrder.Length;i++){
				if(PlayerOrder[i].hasActed == false){
					done = false;
					PlayerOrder[i].hasActed = true;
					PlayerOrder[i].isActing = true;
					GRefs.battleUnitManager.SelectMech(PlayerOrder[i].unit);
					if(PlayerOrder[i].unit.team != 0){
						//AI turn
						GlobalFuncs.PostMessage("Doing AI turn for " + PlayerOrder[i].unit.unitName);
						if(battleState == GEnums.EBattleState.MovingSelectNext)
							GRefs.battleUnitManager.FinishMove(PlayerOrder[i].unit);
						else
							GRefs.battleUnitManager.FinishShooting(PlayerOrder[i].unit);
						// Globals.SetBattleState(GEnums.EBattleState.MovingSelectNext);
						return;
					}
					if(battleState == GEnums.EBattleState.MovingSelectNext)
						Globals.SetBattleState(GEnums.EBattleState.MovingWaitingForInput);
					else if(battleState == GEnums.EBattleState.ShootingSelectNext)
						Globals.SetBattleState(GEnums.EBattleState.ShootingWaitingForInput);
					return;
				}
			}
			if(done){
				GRefs.battleUnitManager.UnselectAllUnits();
				GRefs.btUnitDisplayManager.ResetSelections(true,true);
				if (battleState != GEnums.EBattleState.MovingSelectNext)
					Globals.SetBattleState(GEnums.EBattleState.PhysicalInit);
				else
					Globals.SetBattleState(GEnums.EBattleState.ShootingInit);
			}
		}else if(battleState == GEnums.EBattleState.ShootingInit){
			InitPhase(-1);
			Globals.SetBattleState(GEnums.EBattleState.ShootingSelectNext);
			GRefs.battleUnitManager.NextPhase();
		}
    }

	void InitPhase(int direction){
		System.Random rnd = new System.Random();

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
				int index = rnd.Next(0,shortList.Count);
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
