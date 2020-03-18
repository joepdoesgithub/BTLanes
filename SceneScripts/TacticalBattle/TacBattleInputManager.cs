using UnityEngine;

public class TacticalBattleInputManager : MonoBehaviour{
	void Update(){
		if(Input.GetKeyDown(KeyCode.Space))
			GlobalFuncs.PostMessage("Space!");


		// if(Globals.animationState != Globals.AnimationState.NoAnim)
		// 	return;
		// if(Globals.noInputTime)
		// 	return;

		// if(Input.GetKeyDown(KeyCode.Return)){
		// 	// Globals.PostMessage("Enter is pressed" + Time.time.ToString());
		// 	Globals.gameManager.DoNextMove();
		// }else 

		// if(Input.GetKeyDown(KeyCode.Slash))
		// 	Globals.cameraManager.SwitchZoom();
		
		// // else if(Input.GetKeyDown(KeyCode.Space)){
		// // 	Globals.cameraManager.TestFunc();
		// // 	Globals.PostMessage("Space");
		// // }

		// // Targeting
		// else if(Input.GetKeyDown(KeyCode.T)){
		// 	Globals.targetingManager.TargetingKeyPressed();
		// 	// GameObject.Find("ShootingManager").GetComponent<TargetingManager>().TargetingKeyPressed();
		// }
		// else if(Input.GetKeyDown(KeyCode.F)){
		// 	Debug.Log("jkl;");
		// 	Globals.targetingManager.SwitchFireMode();
		// }for(int i = 0;i<10;i++){
		// 	if(Input.GetKeyDown("" + i))
		// 		Globals.targetingManager.WeaponkeyPressed(i);
		// }

		// // Changing speed
		// if(Input.GetKeyDown(KeyCode.W))
		// 	Globals.playerUnitScript.ChangeSpeed(1);
		// else if(Input.GetKeyDown(KeyCode.S))
		// 	Globals.playerUnitScript.ChangeSpeed(-1);

		// // Changing facing
		// else if(Input.GetKeyDown(KeyCode.A))
		// 	Globals.playerUnitScript.ChangeDirection(-1);
		// else if(Input.GetKeyDown(KeyCode.D))
		// 	Globals.playerUnitScript.ChangeDirection(1);

		// // Changing TorsoTwist
		// else if(Input.GetKeyDown(KeyCode.Q))
		// 	Globals.playerUnitScript.ChangeTorso(-1);
		// else if(Input.GetKeyDown(KeyCode.E))
		// 	Globals.playerUnitScript.ChangeTorso(1);
	}
}