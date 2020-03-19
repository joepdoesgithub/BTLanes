using UnityEngine;

public class TacBattleInputManager : MonoBehaviour{
	GEnums.EBattleState battleState;

	void Update(){
		battleState = Globals.GetBattleState();
		if(GRefs.battleManager == null ||
				(!(battleState == GEnums.EBattleState.MovingWaitingForInput ||
				battleState == GEnums.EBattleState.ShootingWaitingForInput)) )
			return;



		if(battleState == GEnums.EBattleState.MovingWaitingForInput){
			if(Input.GetKeyDown(KeyCode.A)){
				GRefs.battleUnitManager.MoveUnitLeft();
			}else
			if(Input.GetKeyDown(KeyCode.D)){
				GRefs.battleUnitManager.MoveUnitRight();
			}else
			if(Input.GetKeyDown(KeyCode.Q)){
				GRefs.battleUnitManager.TurnUnitLeft();
			}else
			if(Input.GetKeyDown(KeyCode.Z)){
				GRefs.battleUnitManager.ResetUnit();
			}else
			if(Input.GetKeyDown(KeyCode.E)){
				GRefs.battleUnitManager.TurnUnitRight();
			}
		}else if(battleState == GEnums.EBattleState.ShootingWaitingForInput){
			if(Input.GetKeyDown(KeyCode.F)){
				GRefs.battleUnitManager.FireSelectedWeaponAtTarget();
			}else
			if(Input.GetKeyDown(KeyCode.Z)){
				GRefs.battleUnitManager.ResetShooting();
			}
		}
	
		if(Input.GetKeyDown(KeyCode.Return)){
			if(battleState == GEnums.EBattleState.MovingWaitingForInput){
				GRefs.battleUnitManager.FinishMove();
				Globals.SetBattleState(GEnums.EBattleState.MovingSelectNext);
			}else if(battleState == GEnums.EBattleState.ShootingWaitingForInput){
				GRefs.battleUnitManager.FinishShooting();
				Globals.SetBattleState(GEnums.EBattleState.ShootingSelectNext);
			}
		}else
		if(Input.GetKeyDown(KeyCode.Space)){
			GlobalFuncs.PostMessage("Space!");
		}else
		if(Input.GetKeyDown(KeyCode.T)){
			GRefs.btUnitDisplayManager.SelectEnemyMech();
		}else
		if(Input.GetKeyDown(KeyCode.Tab)){
			GRefs.btUnitDisplayManager.TabWeapon();
		}
	}
}