using System.Collections.Generic;
using UnityEngine;

public static class BTDamageHelper{
	// determine which close range to pick by checking which side is most vulnerable (least sum of(struct+arm))

	public static void ResolveDamage(int dist, int shooterFacing, int enemyFacing, int damage, int enemyID, out string critString){
		System.Random rnd = new System.Random();
		int r = rnd.Next(1,7) + rnd.Next(1,7);
		Unit enemy = GLancesAndUnits.GetUnit(enemyID);
		
		enemy.DArmourPoints[ normalShooting[r] ] -= damage;



		critString = string.Format("Hit {0} for {1} dmg now at {2}",
			normalShooting[r],
			damage,
			enemy.DArmourPoints[ normalShooting[r] ]);
		Debug.Log(critString);
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