using UnityEngine;
using System.Collections.Generic;

public static class WeaponsHelper{
	public static void LoadUnitWeapons(ref Unit unit, string name, TextAsset unitTextData){	
		// Find the number of weapons in the file	
		int numWeapons = UnitsLoader.GetFieldFromTextAssetInt(unitTextData,"Weapons:");
		GEnums.SWeapon[] wpns = new GEnums.SWeapon[numWeapons];
		
		// Parse the weapons
		for(int i = 1;i<=numWeapons;i++){
			// First, get individual weapon from the MechData
			string wpnStringFromUnitData = GetWeaponLineFromUnitTextData(unitTextData,i,"Weapons:");

			// Parse this weapon an get the data from the wpn database
			wpns[i-1] = GetWeaponFromString(wpnStringFromUnitData);
			wpns[i-1].startReloadTime = 0.0f;

			// Finally, get location of weapon
			wpns[i-1].loc = DMekHeaderStringToLok[ wpnStringFromUnitData.Split(',')[1].Trim() ];
		}

		List<int> ids = new List<int>();
		System.Random rnd = new System.Random();
		for(int i = 0;i<wpns.Length;i++){
			int newID = rnd.Next(1000,10000);
			while(ids.Contains(newID))
				newID = rnd.Next(1000,10000);
			wpns[i].ID = newID;
		}

		unit.weapons = wpns;
	}

	// static Globals.SWeapon GetWeaponFromName(string wpnName){return GetWeaponFromString(wpnName + ", ");}
	static GEnums.SWeapon GetWeaponFromString(string wpnLine){
		string wpnName = wpnLine.Split(',')[0];
		TextAsset txt = GetWeaponDataText();

		// Get the correct line from the wpn data file
		string line = "";
		foreach(string s in System.Text.RegularExpressions.Regex.Split(txt.text,"\n|\r|\r\n")){
			if(s.Split(',')[0].Trim() == wpnName)
				line = s;
		}
		if(line == ""){
			Debug.LogError("WeaponsHelper.GetWeaponFromString: Can't find data about " + wpnName);
			return new GEnums.SWeapon();
		}

		// Parse the data into the wpn object
		return WeaponDataParser.ParseDataStringIntoWeapon(line);
	}

	static TextAsset GetWeaponDataText(){
		string filePath = "GameData/Weapons";
		TextAsset textFile = Resources.Load<TextAsset>(filePath);
		if(textFile == null)
			Debug.LogError(string.Format("WeaponsHelper.GetWeaponText: 2. Can't load text data for {0}",filePath));
		return textFile;
	}

	static string GetWeaponLineFromUnitTextData(TextAsset text, int disp, string searchTerm){
		string[] lines = System.Text.RegularExpressions.Regex.Split(text.text,"\n|\r|\r\n");
		for(int i = 0;i<lines.Length;i++){
			if(lines[i].Contains(searchTerm))
				return lines[i+disp];
		}
		return "";
	}

	// public static void PrintWeapon(Globals.SWeapon wpn){
	// 	string s = wpn.name;
	// 	s += "," + wpn.type;
	// 	s += ",[" + string.Format("{0}/{1}/{2}/{3}]",wpn.ranges[0],wpn.ranges[1],wpn.ranges[2],wpn.ranges[3]);
	// 	// s += "," + wpn.type;
	// 	// s += "," + wpn.type;
	// 	// s += "," + wpn.type;
	// 	// s += "," + wpn.type;
	// 	// s += "," + wpn.type;
	// 	// s += "," + wpn.type;
	// 	Debug.Log(s);
	// }

	static Dictionary<GEnums.EMechLocation,string> DMekLokToHeaderString = new Dictionary<GEnums.EMechLocation, string>{
			{GEnums.EMechLocation.LA,"Left Arm:"},
			{GEnums.EMechLocation.RA,"Right Arm:"},
			{GEnums.EMechLocation.LT,"Left Torso:"},
			{GEnums.EMechLocation.RT,"Right Torso:"},
			{GEnums.EMechLocation.CT,"Center Torso:"},
			{GEnums.EMechLocation.HD,"Head:"},
			{GEnums.EMechLocation.LL,"Left Leg:"},
			{GEnums.EMechLocation.RL,"Right Leg:"} };
	static Dictionary<string,GEnums.EMechLocation> DMekHeaderStringToLok = new Dictionary<string,GEnums.EMechLocation>{
			{"Left Arm",GEnums.EMechLocation.LA},
			{"Right Arm",GEnums.EMechLocation.RA},
			{"Left Torso",GEnums.EMechLocation.LT},
			{"Right Torso",GEnums.EMechLocation.RT},
			{"Center Torso",GEnums.EMechLocation.CT},
			{"Head",GEnums.EMechLocation.HD},
			{"Left Leg",GEnums.EMechLocation.LL},
			{"Right Leg",GEnums.EMechLocation.RL} };
}