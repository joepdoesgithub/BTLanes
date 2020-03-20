using UnityEngine;

public class BTShootingHelper{
	Unit selectedUnit;
	System.Random rnd;

	SWeaponToFire[] wpns;

	public BTShootingHelper(Unit unit){
		selectedUnit = unit;
		rnd = new System.Random();
		wpns = new SWeaponToFire[unit.weapons.Length];
		for(int i = 0; i<wpns.Length;i++)
			wpns[i] = new SWeaponToFire(unit.weapons[i].ID);
	}

	public void FinalizeShooting(){
		foreach(SWeaponToFire w in wpns){
			if(w.hasFired && w.targetID > 0){
				GEnums.SWeapon wpn = GetWpnFromID(w.weaponID);
				Debug.LogFormat("{0} at {1}",GetSWeaponFromID(w.weaponID),GLancesAndUnits.GetUnit(w.targetID));

				int shooterLaneNum = GRefs.battleUnitManager.GetSelectedUnitLaneNum();
				int targetLaneNum = GRefs.battleUnitManager.GetSelectedEnemyLaneNum();
				int dist = Mathf.Abs(targetLaneNum - shooterLaneNum);

				int gunnery = GRefs.battleUnitManager.GetSelectedUnit().pilot.Gunnery;

				int rangeMod = 0;
				if(wpn.ranges[0] > 0 && dist <= wpn.ranges[0])
					rangeMod = (wpn.ranges[0] - dist + 1) * GGameStats.RangeModifier[0];
				else{
					for(int i = 0;i<4;i++){
						if(dist <= wpn.ranges[i]){
							rangeMod = GGameStats.RangeModifier[i];
							break;
						}
					}
				}
				
				int toHitMod = GRefs.battleUnitManager.GetSelectedUnit().toHitModifier;
				int toBeHitMod = GRefs.btUnitDisplayManager.GetSelectedEnemy().toBeHitModifier;
				int terrainMod = 0;
				int secondTargetMod = 0;
				int armMod = 0;

				int targetNum = gunnery + rangeMod + toHitMod + toBeHitMod + terrainMod + secondTargetMod + armMod;
				int roll = rnd.Next(1,7) + rnd.Next(1,7);

				string s = string.Format("Firing {0} at {1}. Needs a {2}, rolled {3}.",wpn.name,GRefs.battleUnitManager.GetSelectedUnit().unitName,targetNum,roll);

				if(roll >= targetNum)
					s += " Hit! Doing " + wpn.damage + " damage";
				else
					s += " Miss.";
				GlobalFuncs.PostMessage(s);
			}
		}
	}

	public void Reset(){
		wpns = new SWeaponToFire[selectedUnit.weapons.Length];
		for(int i = 0; i<wpns.Length;i++)
			wpns[i] = new SWeaponToFire(selectedUnit.weapons[i].ID);
	}

	public bool HasWeaponFiredFromID(int weaponID){
		if(selectedUnit==null || wpns == null)
			return false;
		return GetSWeaponFromID(weaponID).hasFired;
	}

	public void ShootWeaponAtTargetID(int weaponID, int targetID){
		if(selectedUnit == null){
			Debug.LogError("BTShootingHelper.ShootWeaponAtTarget: No unit selected");
			return;
		}
		int wpnIndex = GetWpnsIndexFromID(weaponID);		
		if( wpnIndex < 0){
			Debug.LogError("BTShootingHelper.ShootWeaponAtTarget: WeaponID not found");
			return;
		}

		int shooterLaneNum = GRefs.battleUnitManager.GetSelectedUnitLaneNum();
		int targetLaneNum = GRefs.battleUnitManager.GetSelectedEnemyLaneNum();
		int dist = Mathf.Abs(targetLaneNum - shooterLaneNum);
		if(dist > GetWpnFromID(weaponID).ranges[3])
			return;

		bool weaponHadAlreadyFired = false;
		if(wpns[wpnIndex].hasFired){
			if(wpns[wpnIndex].targetID == targetID)
				return;
			weaponHadAlreadyFired = true;
		}

		wpns[wpnIndex].hasFired = true;
		wpns[wpnIndex].targetID = targetID;

		// string targetName = GLancesAndUnits.GetUnit(targetID).unitName;
		// string wpnName = "";
		if(!weaponHadAlreadyFired){
			foreach(GEnums.SWeapon w in selectedUnit.weapons){
				if(w.ID == weaponID){
					// wpnName = w.name;
					GRefs.battleUnitManager.heat += (int)w.heat;
				}
			}
		}

		// GlobalFuncs.PostMessage(string.Format("Firing {0},ID:{1} at {2}",wpnName,weaponID,targetName));
	}

	SWeaponToFire GetSWeaponFromID(int id){
		foreach(SWeaponToFire s in wpns){
			if(s.weaponID == id)
				return s;
		}
		return new SWeaponToFire(-1);
	}
	int GetWpnsIndexFromID(int id){
		for(int i = 0;i<wpns.Length;i++){
			if(wpns[i].weaponID == id)
				return i;
		}
		return -1;
	}
	GEnums.SWeapon GetWpnFromID(int id){
		for(int i = 0;i<selectedUnit.weapons.Length;i++){
			if(selectedUnit.weapons[i].ID == id)
				return selectedUnit.weapons[i];
		}
		return new GEnums.SWeapon();
	}

	////////////////////
	//
	//		Statics
	//
	////////////////////
	// public static bool IsWeaponInRange(Unit shooter, Unit target, int weaponID){
		
	// }

	struct SWeaponToFire{
		public int weaponID;
		public int targetID;
		public bool hasFired;

		public SWeaponToFire(int weaponID){
			this.weaponID = weaponID;
			targetID = -1;
			hasFired = false;
		}
	}
}