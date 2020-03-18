using UnityEngine;

public class BTShootingHelper{
	Unit selectedUnit;

	SWeaponToFire[] wpns;

	public BTShootingHelper(Unit unit){
		selectedUnit = unit;
		wpns = new SWeaponToFire[unit.weapons.Length];
		for(int i = 0; i<wpns.Length;i++)
			wpns[i] = new SWeaponToFire(unit.weapons[i].ID);
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

		wpns[wpnIndex].hasFired = true;
		wpns[wpnIndex].targetID = targetID;

		string targetName = GLancesAndUnits.GetUnit(targetID).unitName;
		string wpnName = "";
		foreach(GEnums.SWeapon w in selectedUnit.weapons){
			if(w.ID == weaponID)
				wpnName = w.name;
		}

		GlobalFuncs.PostMessage(string.Format("Firing {0},ID:{1} at {2}",wpnName,weaponID,targetName));
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