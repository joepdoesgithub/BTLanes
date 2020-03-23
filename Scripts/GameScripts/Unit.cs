using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit{
	public Pilot pilot;

	public int btWalkSpeed = 3;
	public int walkSpeed;
	public int runSpeed;

	public string unitName;
	public int tonnage;
	public int heat = 0;
	public string heatSinks;
	public int heatSinking;

	public int team;
	public int ID;

	public bool stationary;
	public bool ran;
	public bool jumped;
	public int toHitModifier;
	public int toBeHitModifier;

	public Dictionary<GEnums.EMechLocation,int> DStructucePoints;
	public Dictionary<GEnums.EMechLocation,int> DArmourPoints;

	public GEnums.SWeapon[] weapons;

	public Unit(){
		pilot = new Pilot();
	}

	public bool IsUnitDestroyed(){
		if(DStructucePoints[GEnums.EMechLocation.HD] <= 0 || DStructucePoints[GEnums.EMechLocation.CT] <= 0)
			return true;
		return false;
	}
}
