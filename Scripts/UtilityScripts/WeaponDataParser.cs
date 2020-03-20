using UnityEngine;
using System.Collections.Generic;

public static class WeaponDataParser{
	static string[] fields;

	public static GEnums.SWeapon ParseDataStringIntoWeapon(string wpnData){
		GEnums.SWeapon wpn = new GEnums.SWeapon();

		fields = wpnData.Split(',');

		wpn.name = GetField("Name");
		GetWeaponType(ref wpn,fields[1],fields[2]);
		wpn.heat = GetFieldF("Heat");
		SetWeaponRanges(ref wpn);
		wpn.damage = GetFieldF("Damage");
		wpn.rechargeTime = GetFieldF("RechargeTime");
		SetWeaponMissileStats(ref wpn);

		return wpn;
	}

	static void SetWeaponRanges(ref GEnums.SWeapon wpn){
		wpn.ranges = new int[4];
		wpn.ranges[0] = (int)(GetFieldF("MinRange")/2 +.5f);
		wpn.ranges[0] = (wpn.ranges[0] <= 0? -1 : wpn.ranges[0]);
		wpn.ranges[1] = (int)(GetFieldF("ShortRange")/2 +.5f);
		wpn.ranges[2] = (int)(GetFieldF("MediumRange")/2 +.5f);
		wpn.ranges[3] = (int)(GetFieldF("LongRange")/2 +.5f);

		if(wpn.ranges[1] == wpn.ranges[2])
			wpn.ranges[1]--;
	}

	static void SetWeaponMissileStats(ref GEnums.SWeapon wpn){
		if(wpn.type == GEnums.EWeaponType.Energy || wpn.type == GEnums.EWeaponType.Ballistic){
			wpn.dmgPerMissile = 0;
			wpn.missileGroupSize = 0;
		}else{
			if(wpn.type == GEnums.EWeaponType.MissileLRM){
				wpn.dmgPerMissile = 1;
				wpn.missileGroupSize = 5;
			}else if(wpn.type == GEnums.EWeaponType.MissileSRM){
				wpn.dmgPerMissile = 2;
				wpn.missileGroupSize = 1;
			}
		}
	}

	static void GetWeaponType(ref GEnums.SWeapon wpn, string family, string subfamily){
		if(family == "Energy")
			wpn.type = GEnums.EWeaponType.Energy;
		else if(family == "Ballistic")
			wpn.type = GEnums.EWeaponType.Ballistic;
		else if(family == "Missile"){
			if(subfamily == "LRM")
				wpn.type = GEnums.EWeaponType.MissileLRM;
			else if(subfamily == "SRM")
				wpn.type = GEnums.EWeaponType.MissileSRM;
			else
				Debug.LogError(string.Format("WeaponDataParser.GetWeaponType: Unknown missile subfamily {0} {1}",family,subfamily));
		}else
			Debug.LogError(string.Format("WeaponDataParser.GetWeaponType: Unknown family {0} {1}",family,subfamily));
	}

	static string GetField(string name){return fields[ DFieldNameToIndex[name] ];}
	static float GetFieldF(string name){return float.Parse(GetField(name));}

	static Dictionary<string,int> DFieldNameToIndex = new Dictionary<string,int>{
		{"Name",0},
		{"WeaponFamily",1},
		{"Subfamily",2},
		{"Heat",3},
		{"MinRange",4},
		{"ShortRange",5},
		{"MediumRange",6},
		{"LongRange",7},
		{"Damage",8},
		{"RechargeTime",9},
		{"DmgPerMissile",10},
		{"MissileGroupSize",11} };

}