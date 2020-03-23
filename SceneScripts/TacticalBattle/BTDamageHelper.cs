using System.Collections.Generic;
using System;
using UnityEngine;

public static class BTDamageHelper{
	static int Get2D6(){return UnityEngine.Random.Range(1,7) + UnityEngine.Random.Range(1,7);}


	// determine which close range to pick by checking which side is most vulnerable (least sum of(struct+arm))

	static Unit enemyUnit;
	static bool NotDestroyed;

	public static void ResolveDamage(int dist, int shooterFacing, int enemyFacing, int weaponID, int shooterID, int enemyID, out string critString){
		int r = Get2D6();

		enemyUnit = GLancesAndUnits.GetUnit(enemyID);
		NotDestroyed = !enemyUnit.IsUnitDestroyed();
		GEnums.SWeapon wpn = BTShootingHelper.GetWpnFromID(shooterID,weaponID);
		int damage = (int)wpn.damage;
		if(wpn.type == GEnums.EWeaponType.MissileLRM || wpn.type == GEnums.EWeaponType.MissileSRM){
			// critString = "Do missile stuff!!!";
			// return;
			damage = (int)wpn.dmgPerMissile * wpn.missileGroupSize;
		}
		
		// Check which location is hit!
		GEnums.EMechLocation locHit = normalShooting[ r ];
		if(dist <= 2){
			int side = GetCloseRangeSideChooser();
			if(side < 0)
				locHit = leftShooting[ r ];
			else
				locHit = rightShooting[ r ];
		}
		if( (shooterFacing<0 && enemyFacing<0) || (shooterFacing>0 && enemyFacing>0) ){
			if(locHit == GEnums.EMechLocation.RT)
				locHit = GEnums.EMechLocation.RTR;
			else if(locHit == GEnums.EMechLocation.LT)
				locHit = GEnums.EMechLocation.RTL;
			else if(locHit == GEnums.EMechLocation.CT)
				locHit = GEnums.EMechLocation.RTC;
		}

		// Do the damage
		// string pre = GetArmStrString(enemyID);
		critString = DoTheDamage(locHit, damage);
		// string post = GetArmStrString(enemyID);
		// Debug.LogFormat("{3} to the {4}\n{0}\n{1}\n{2}",pre,critString,post,wpn.name,locHit.ToString());
	}

	static string DoTheDamage(GEnums.EMechLocation loc, int damage){
		GEnums.EMechLocation structLoc = loc;
		if(loc == GEnums.EMechLocation.RTR)
			structLoc = GEnums.EMechLocation.RT;
		else if(loc == GEnums.EMechLocation.RTC)
			structLoc = GEnums.EMechLocation.CT;
		else if(loc == GEnums.EMechLocation.RTL)
			structLoc = GEnums.EMechLocation.LT;
		
		int armourRemaining = enemyUnit.DArmourPoints[loc];
		int structRemaining = enemyUnit.DStructucePoints[structLoc];

		if(structRemaining == 0 && (structLoc == GEnums.EMechLocation.CT || structLoc == GEnums.EMechLocation.HD) )
			return string.Format("No further damage done, the {0} is already destroyed.",
					(structLoc == GEnums.EMechLocation.HD ? "head" : "center torso"));

		string s = "";
		int dmgRem = damage;

		if(armourRemaining > 0){
			int dmgDone = (damage <= armourRemaining ? damage : armourRemaining);
			s += string.Format("Took {0} to the {1} armour.",dmgDone,loc.ToString());
			enemyUnit.DArmourPoints[loc] -= dmgDone;
			dmgRem -= dmgDone;
		}

		if(dmgRem > 0 && structRemaining > 0){
			int dmgDone = (dmgRem <= structRemaining ? dmgRem : structRemaining);
			s += (s.Length > 0 ? " And t" : "T");
			s += string.Format("ook {0} to the {1} structure,",dmgDone,loc.ToString());
			enemyUnit.DStructucePoints[structLoc] -= dmgDone;
			s += (enemyUnit.DStructucePoints[structLoc] <= 0 && NotDestroyed ? " destroying it." : string.Format(" {0} struct remaining.",enemyUnit.DStructucePoints[structLoc]));
			dmgRem -= dmgDone;
		}

		if(enemyUnit.DStructucePoints[structLoc] <= 0 && NotDestroyed){
			if(loc == GEnums.EMechLocation.HD){
				s += " The head is destroyed and the mech collapses.";
				GRefs.battleUnitManager.DestroyUnit(enemyUnit.ID);
				dmgRem = 0;
				return s;
			}else if(structLoc == GEnums.EMechLocation.CT){
				s += " The mech is cored, falling to the ground a ruined pile of rubble.";
				GRefs.battleUnitManager.DestroyUnit(enemyUnit.ID);
				dmgRem = 0;
				return s;
			}else if(structLoc == GEnums.EMechLocation.RT && enemyUnit.DStructucePoints[GEnums.EMechLocation.RA] > 0){
				s += " The right torso is destroyed and its connected arm flies off";
				enemyUnit.DStructucePoints[GEnums.EMechLocation.RA] = 0;
				enemyUnit.DArmourPoints[GEnums.EMechLocation.RA] = 0;
			}else if(structLoc == GEnums.EMechLocation.LT && enemyUnit.DStructucePoints[GEnums.EMechLocation.LA] > 0){
				s += " The left torso is destroyed and its connected arm flies off";
				enemyUnit.DStructucePoints[GEnums.EMechLocation.LA] = 0;
				enemyUnit.DArmourPoints[GEnums.EMechLocation.LA] = 0;
			}
		}

		if(dmgRem > 0)
			return s + " " + DoTheDamage( NextLocation(loc), dmgRem );
		return s;
	}

	static GEnums.EMechLocation NextLocation(GEnums.EMechLocation loc){
		switch (loc){
			case GEnums.EMechLocation.LA:
			case GEnums.EMechLocation.LL:
				return GEnums.EMechLocation.LT;
			case GEnums.EMechLocation.RA:
			case GEnums.EMechLocation.RL:
				return GEnums.EMechLocation.RT;
			case GEnums.EMechLocation.RT:
			case GEnums.EMechLocation.LT:
			default:
				return GEnums.EMechLocation.CT;
		}
	}

	static int GetCloseRangeSideChooser(){
		int left = 0;
		int right = 0;

		string s;
		foreach(GEnums.EMechLocation loc in Enum.GetValues(typeof(GEnums.EMechLocation))){
			s = loc.ToString();
			if(s.Length == 3){
				if(loc == GEnums.EMechLocation.RTL)
					left += enemyUnit.DArmourPoints[loc];
				else if(loc == GEnums.EMechLocation.RTR)
					right += enemyUnit.DArmourPoints[loc];
			}else if(s.Substring(0,1) == "L"){
				left += enemyUnit.DArmourPoints[loc];
				left += enemyUnit.DStructucePoints[loc];
			}else if(s.Substring(0,1) == "R"){
				right += enemyUnit.DArmourPoints[loc];
				right += enemyUnit.DStructucePoints[loc];
			}
		}
		if(left > right)
			return 1;
		else if(right > left)
			return -1;
		return (UnityEngine.Random.Range(1,11)%2==0?-1:1);
	}

	static string GetArmStrString(int unitID){
		Unit unit = GLancesAndUnits.GetUnit(unitID);
		string s = "";
		foreach(GEnums.EMechLocation l in Enum.GetValues(typeof(GEnums.EMechLocation))){
			s += l.ToString() + ",";
			if(l == GEnums.EMechLocation.RTL || l == GEnums.EMechLocation.RTC || l == GEnums.EMechLocation.RTR)
				s += unit.DArmourPoints[l] + ", ";
			else
				s += string.Format("{0},{1}, ",unit.DArmourPoints[l],unit.DStructucePoints[l]);
		}
		return s;
	}

	readonly static Dictionary<int,GEnums.EMechLocation> normalShooting = new Dictionary<int, GEnums.EMechLocation>(){
		{ 2,GEnums.EMechLocation.CT},
		{ 3,GEnums.EMechLocation.RA},
		{ 4,GEnums.EMechLocation.RA},
		{ 5,GEnums.EMechLocation.RL},
		{ 6,GEnums.EMechLocation.RT},
		{ 7,GEnums.EMechLocation.CT},
		{ 8,GEnums.EMechLocation.LT},
		{ 9,GEnums.EMechLocation.LL},
		{10,GEnums.EMechLocation.LA},
		{11,GEnums.EMechLocation.LA},
		{12,GEnums.EMechLocation.HD},
	};
	readonly static Dictionary<int,GEnums.EMechLocation> leftShooting = new Dictionary<int, GEnums.EMechLocation>(){
		{ 2,GEnums.EMechLocation.LT},
		{ 3,GEnums.EMechLocation.LL},
		{ 4,GEnums.EMechLocation.LA},
		{ 5,GEnums.EMechLocation.LA},
		{ 6,GEnums.EMechLocation.LL},
		{ 7,GEnums.EMechLocation.LT},
		{ 8,GEnums.EMechLocation.CT},
		{ 9,GEnums.EMechLocation.RT},
		{10,GEnums.EMechLocation.RA},
		{11,GEnums.EMechLocation.RL},
		{12,GEnums.EMechLocation.HD},
	};
	readonly static Dictionary<int,GEnums.EMechLocation> rightShooting = new Dictionary<int, GEnums.EMechLocation>(){
		{ 2,GEnums.EMechLocation.RT},
		{ 3,GEnums.EMechLocation.RL},
		{ 4,GEnums.EMechLocation.RA},
		{ 5,GEnums.EMechLocation.RA},
		{ 6,GEnums.EMechLocation.RL},
		{ 7,GEnums.EMechLocation.RT},
		{ 8,GEnums.EMechLocation.CT},
		{ 9,GEnums.EMechLocation.LT},
		{10,GEnums.EMechLocation.LA},
		{11,GEnums.EMechLocation.LL},
		{12,GEnums.EMechLocation.HD},
	};
}