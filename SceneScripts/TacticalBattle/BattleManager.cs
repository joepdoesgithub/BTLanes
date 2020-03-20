using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour{
	SPlayerOrder[] PlayerOrder;
	public SPlayerOrder[] GetPlayerOrders(){return PlayerOrder;}

	GEnums.EBattleState battleState;

	void Start(){GRefs.battleManager = this;}

    // Update is called once per frame
	bool movingSelectNext;
	bool shootingSelectNext;
    void Update(){
		battleState = Globals.GetBattleState();
		movingSelectNext = (battleState == GEnums.EBattleState.MovingSelectNext);
		shootingSelectNext = (battleState == GEnums.EBattleState.ShootingSelectNext);

		if(battleState == GEnums.EBattleState.AllInit)
			return;
        else if(battleState == GEnums.EBattleState.MovingInit){
			InitPhase(1);
			GRefs.battleUnitManager.NextPhase(battleState);
			Globals.SetBattleState(GEnums.EBattleState.MovingSelectNext);
		}else if(movingSelectNext || shootingSelectNext){
			bool done = true;
			for(int i = 0;i<PlayerOrder.Length;i++){
				if(PlayerOrder[i].hasActed == false){
					done = false;
					PlayerOrder[i].hasActed = true;
					PlayerOrder[i].isActing = true;
					GRefs.battleUnitManager.SelectMech(PlayerOrder[i].unit);
					if(PlayerOrder[i].unit.team != 0){
						//AI turn
						GlobalFuncs.PostMessage(string.Format("Doing AI {0} turn for " + PlayerOrder[i].unit.unitName, (movingSelectNext?"moving":"shooting") ));
						if(movingSelectNext)
							GRefs.battleUnitManager.FinishMove(PlayerOrder[i].unit);
						else
							GRefs.battleUnitManager.FinishShooting(PlayerOrder[i].unit);
						// Globals.SetBattleState(GEnums.EBattleState.MovingSelectNext);
						return;
					}
					if(movingSelectNext)
						Globals.SetBattleState(GEnums.EBattleState.MovingWaitingForInput);
					else if(shootingSelectNext)
						Globals.SetBattleState(GEnums.EBattleState.ShootingWaitingForInput);
					return;
				}
			}
			if(done){
				GRefs.battleUnitManager.UnselectAllUnits();
				GRefs.btUnitDisplayManager.ResetSelections(true,true);
				if (movingSelectNext)
					Globals.SetBattleState(GEnums.EBattleState.ShootingInit);
				else if (shootingSelectNext){
					// Globals.SetBattleState(GEnums.EBattleState.PhysicalInit);
					Globals.SetBattleState(GEnums.EBattleState.MovingInit);
				}
			}
		}else if(battleState == GEnums.EBattleState.ShootingInit){
			InitPhase(-1);
			Globals.SetBattleState(GEnums.EBattleState.ShootingSelectNext);
			GRefs.battleUnitManager.NextPhase(battleState);
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
