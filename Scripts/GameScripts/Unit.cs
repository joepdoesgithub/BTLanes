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

	public int team;
	public int ID;

	public Dictionary<GEnums.EMechLocation,int> DStructucePoints;
	public Dictionary<GEnums.EMechLocation,int> DArmourPoints;

	public GEnums.SWeapon[] weapons;

	public Unit(){
		pilot = new Pilot();
	}
}
