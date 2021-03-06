using System.Collections.Generic;
using UnityEngine;

public static class GLancesAndUnits{
	public static List<Unit> units = null;
	public static List<Lance> lances = null;

	public static Unit GetUnit(int id){
		if(units == null)
			return null;
		foreach(Unit u in units){
			if(u.ID == id)
				return u;
		}
		string s = "Finding " + id.ToString() + " in: ";
		foreach(Unit u in units)
			s += u.ID.ToString() + ", ";
		Debug.LogError("GLancesAndUnits.GetUnit: Can't find unit. " + s);
		return null;
	}

	public static List<int> GetAllWeaponIDs(){
		List<int> l = new List<int>();
		if(units == null || units.Count <= 0)
			return l;

		foreach(Unit u in units){
			foreach(GEnums.SWeapon wpn in u.weapons)
				l.Add(wpn.ID);
		}
		return l;
	}

	public static List<GEnums.SWeapon> GetAllWeapons(){
		List<GEnums.SWeapon> l = new List<GEnums.SWeapon>();
		if(units == null || units.Count <= 0)
			return l;

		foreach(Unit u in units){
			foreach(GEnums.SWeapon wpn in u.weapons)
				l.Add(wpn);
		}
		return l;
	}


	public static GEnums.SWeapon GetWeapon(int wpnID){
		foreach(GEnums.SWeapon wpn in GetAllWeapons()){
			if(wpn.ID == wpnID)
				return wpn;
		}
		return new GEnums.SWeapon();
	}
}

public class Lance{
	public int team;
	public string name;
	public Unit[] units;

	public int GetLength(){return units.Length;}
	public Lance(){team = -1; name = "";}
	public Lance(int team, string name){this.team = team;this.name = name;}
	public void SwitchOrder(int from, int to){
		Unit[] newUnits = new Unit[GetLength()];
		for(int i = 0;i<GetLength();i++){
			if(i == from)
				newUnits[i] = units[to];
			else if(i == to)
				newUnits[i] = units[from];
			else
				newUnits[i] = units[i];
		}
		units = newUnits;
	}
	public void AddUnitToLance(Unit newUnit){
		if(newUnit.team != this.team){
			GlobalFuncs.PostMessage("GLancesAndUnits.AddUnitToLance: unit-team doesn't match the lance-team");
			return;
		}
		if(units == null)
			units = new Unit[0];
		
		Unit[] newUnits = new Unit[units.Length + 1];
		for(int i =0;i<units.Length;i++)
			newUnits[i] = units[i];
		newUnits[newUnits.Length-1] = newUnit;
		units = newUnits;
	}
}