using UnityEngine;
using System.Collections.Generic;
using System;

public static class UnitStructArmourHelper{
	public static void SetStructArmour(ref Unit unit, string name){
		unit.DStructucePoints = new Dictionary<GEnums.EMechLocation, int>();
		unit.DArmourPoints = new Dictionary<GEnums.EMechLocation, int>();

		int tonnage = unit.tonnage;
		LoadStructPoints(ref unit.DStructucePoints,tonnage);
		LoadArmourPoints(ref unit.DArmourPoints,name);

		unit.armourMax = unit.structureMax = 0;
		foreach(GEnums.EMechLocation l in Enum.GetValues(typeof(GEnums.EMechLocation))){
			unit.armourMax += unit.DArmourPoints[l];
			if( !(l == GEnums.EMechLocation.RTC || l == GEnums.EMechLocation.RTL || l == GEnums.EMechLocation.RTR))
				unit.structureMax += unit.DStructucePoints[l];
		}

	}

	static void LoadArmourPoints(ref Dictionary<GEnums.EMechLocation,int> dict, string name){
		TextAsset text = UnitsLoader.GetUnitTextasset(name);
		
		dict.Add(GEnums.EMechLocation.HD, UnitsLoader.GetFieldFromTextAssetInt(text,"HD Armor:") );
		dict.Add(GEnums.EMechLocation.CT, UnitsLoader.GetFieldFromTextAssetInt(text,"CT Armor:") );
		dict.Add(GEnums.EMechLocation.RT, UnitsLoader.GetFieldFromTextAssetInt(text,"RT Armor:") );
		dict.Add(GEnums.EMechLocation.LT, UnitsLoader.GetFieldFromTextAssetInt(text,"LT Armor:") );
		dict.Add(GEnums.EMechLocation.LA, UnitsLoader.GetFieldFromTextAssetInt(text,"LA Armor:") );
		dict.Add(GEnums.EMechLocation.RA, UnitsLoader.GetFieldFromTextAssetInt(text,"RA Armor:") );
		dict.Add(GEnums.EMechLocation.LL, UnitsLoader.GetFieldFromTextAssetInt(text,"LL Armor:") );
		dict.Add(GEnums.EMechLocation.RL, UnitsLoader.GetFieldFromTextAssetInt(text,"RL Armor:") );
		dict.Add(GEnums.EMechLocation.RTL, UnitsLoader.GetFieldFromTextAssetInt(text,"RTL Armor:") );
		dict.Add(GEnums.EMechLocation.RTR, UnitsLoader.GetFieldFromTextAssetInt(text,"RTR Armor:") );
		dict.Add(GEnums.EMechLocation.RTC, UnitsLoader.GetFieldFromTextAssetInt(text,"RTC Armor:") );
	}

	static void LoadStructPoints(ref Dictionary<GEnums.EMechLocation,int> dict, int tonnage){
		TextAsset text = Resources.Load<TextAsset>("GameData/StructureMech");
		if(text == null){
			Debug.LogError(string.Format("UnitStrcutArmourHelper.LoadStructPoints: Can't load text data for {0}","GameData/StructureMech"));
			return;
		}

		string[] lines = System.Text.RegularExpressions.Regex.Split(text.text,"\n|\r|\r\n");
		string line = "";
		foreach(string s in lines){
			if(s.Split(',')[0] == tonnage.ToString())
				line = s;
		}
		if(line == ""){
			Debug.LogError("UnitStrcutArmourHelper.LoadStructPoints: Can't find actual tonnage of unit in data - " + tonnage.ToString());
			return;
		}

		// Tons,HD,CT,T,A,L
		string[] fields = line.Split(',');
		dict.Add(GEnums.EMechLocation.HD,int.Parse(fields[1]));
		dict.Add(GEnums.EMechLocation.CT,int.Parse(fields[2]));
		dict.Add(GEnums.EMechLocation.RT,int.Parse(fields[3]));
		dict.Add(GEnums.EMechLocation.LT,int.Parse(fields[3]));
		dict.Add(GEnums.EMechLocation.LA,int.Parse(fields[4]));
		dict.Add(GEnums.EMechLocation.RA,int.Parse(fields[4]));
		dict.Add(GEnums.EMechLocation.LL,int.Parse(fields[5]));
		dict.Add(GEnums.EMechLocation.RL,int.Parse(fields[5]));
	}
}