using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTCombatPreviewTextManager : MonoBehaviour{
	public Text text;

	int shooterLaneNum, targetLaneNum, dist, facing = 0, weaponID;
	int newShooterLaneNum, newTargetLaneNum, newFacing = 0, newWeaponID;
	GEnums.SWeapon wpn;

	int gunnery,rangeMod,toHitMod,toBeHitMod,armMod,terrainMod,secondTargetMod;
	string s,battlestate;

    // Update is called once per frame
    void Update(){
		battlestate = Globals.GetBattleState().ToString();
		if(battlestate.Contains("Moving"))
			text.text = "";
		else if(battlestate.Contains("Shooting"))
			ShootingUpdate();
	}

	void ShootingUpdate(){
		newShooterLaneNum = GRefs.battleUnitManager.GetSelectedUnitLaneNum();
		if(newShooterLaneNum < 0){
			text.text = "No shooter selected";
			return;
		}
		newTargetLaneNum = GRefs.battleUnitManager.GetSelectedEnemyLaneNum();
		if(newTargetLaneNum < 0){
			text.text = "No target selected";
			return;
		}
		wpn = GRefs.btUnitDisplayManager.GetSelectedWeapon();
		if(wpn.ranges.Length != 4){
			text.text = "No weapon selected";
			return;
		}
		newWeaponID = wpn.ID;

		// in arc?
		// [TODO]

		if(newWeaponID == weaponID && shooterLaneNum == newShooterLaneNum && targetLaneNum == newTargetLaneNum && newFacing == facing)
			return;
		weaponID = newWeaponID;shooterLaneNum = newShooterLaneNum;targetLaneNum = newTargetLaneNum;facing = newFacing;


		dist = Mathf.Abs(shooterLaneNum - targetLaneNum);
		if(dist > wpn.ranges[3]){
			text.text = "Target is out of range";
			return;
		}

		gunnery = GRefs.battleUnitManager.GetSelectedUnit().pilot.Gunnery;
		s = string.Format("{0} (Gunnery)",gunnery);
		
		rangeMod = 0;
		if(wpn.ranges[0] > 0 && dist <= wpn.ranges[0]){
			rangeMod = (wpn.ranges[0] - dist + 1) * GGameStats.RangeModifier[0];
			s += string.Format(" + {0} (MinRange)",rangeMod);
		}else if(dist <= wpn.ranges[1]){
			rangeMod = GGameStats.RangeModifier[1];
			s += (rangeMod != 0? string.Format(" + {0} (ShortRange)",rangeMod):"");
		}else if(dist <= wpn.ranges[2]){
			rangeMod = GGameStats.RangeModifier[2];
			s += string.Format(" + {0} (MedRange)",rangeMod);
		}else{
			rangeMod = GGameStats.RangeModifier[3];
			s += string.Format(" + {0} (LongRange)",rangeMod);
		}

		toHitMod = GRefs.battleUnitManager.GetSelectedUnit().toHitModifier;
		// if(toHitMod > 0)
			s += string.Format(" + {0} (mvmnt)",toHitMod);
		toBeHitMod = GRefs.btUnitDisplayManager.GetSelectedEnemy().toBeHitModifier;
		// if(toBeHitMod > 0)
			s += string.Format(" + {0} (enemy)",toBeHitMod);

		// Modifier voor arm mounted
		armMod = 0;
		// if(armMod != 0)
			s += string.Format(" + {0} (arm)",armMod);
		// Intervening terrain
		terrainMod = 0;
		// if(terrainMod != 0)
			s += string.Format(" + {0} (terrain)",terrainMod);
		// Secondary target
		secondTargetMod = 0;
		s += string.Format(" = {0}",gunnery+rangeMod+toHitMod+toBeHitMod+armMod+terrainMod+secondTargetMod);
		text.text = s;
    }
}
