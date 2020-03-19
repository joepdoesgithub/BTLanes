using UnityEngine;

public static class UnitsLoader{
	public static void InitUnits(string[] unitNames, int team){
		Lance L = new Lance(team,"test" + team);
		if(GLancesAndUnits.units == null)
			GLancesAndUnits.units = new System.Collections.Generic.List<Unit>();
		if(GLancesAndUnits.lances == null)
			GLancesAndUnits.lances = new System.Collections.Generic.List<Lance>();
		foreach(string unitType in unitNames){
			// Unit u = new Unit();
			// LoadUnitFromFile(unitType,ref u);
			Unit u = GenerateUnit(unitType);
			u.team = team;
			GLancesAndUnits.units.Add(u);
			L.AddUnitToLance(u);
		}
		GLancesAndUnits.lances.Add(L);
	}

	public static Sprite GetUnitSprite(Unit unit){
		Sprite unitSprite = Resources.Load<Sprite>("MechSprites/" + unit.unitName);				
		if(unitSprite == null)
			unitSprite = Resources.Load<Sprite>("MechSprites/" + unit.unitName.Split(' ')[0]);
		if(unitSprite == null)
			Debug.LogError(string.Format("UnitsLoader.GetUnitSprie: Can't load image with name {0}",unit.unitName));
		return unitSprite;
	}

	public static Unit GenerateUnit(string name){
		Unit u = new Unit();
		LoadUnitFromFile(name,ref u);
		System.Random rnd = new System.Random();
		int id = -1;
		while(true){
			id = rnd.Next(10000,99999);
			foreach(Unit uu in GLancesAndUnits.units){
				if(uu.ID == id)
					continue;
			}
			break;
		}
		u.ID = id;
		return u;
	}

	public static void LoadUnitFromFile(string unitType, ref Unit unit){
		unit.unitName = unitType;

		TextAsset unitTextData = GetUnitTextasset(unitType);
		unit.btWalkSpeed = GetFieldFromTextAssetInt(unitTextData,TextFieldData.Movement);
		unit.walkSpeed = (int)(unit.btWalkSpeed / 2.0 + 0.5);
		unit.runSpeed = GetRunSpeed(unit.btWalkSpeed);
		unit.tonnage = GetFieldFromTextAssetInt(unitTextData,TextFieldData.Tonnage);
		UnitStructArmourHelper.SetStructArmour(ref unit, unitType);
		WeaponsHelper.LoadUnitWeapons(ref unit, unitType, unitTextData);
	}

	static int GetRunSpeed(int originalWalkSpeed){
		int[] spd = {0,1,2,3,4,4,5,5,6,6,7,8};
		if(originalWalkSpeed < 0)
			return 0;
		if(originalWalkSpeed >= spd.Length)
			return spd[ spd.Length - 1 ];
		return spd[originalWalkSpeed];
	}

	public static TextAsset GetUnitTextasset(string name){
		string filePath = "MechData/" + name;
		TextAsset textFile = Resources.Load<TextAsset>(filePath);
		if(textFile == null)
			Debug.LogError(string.Format("UnitLoader.GetUnitTextAsset: Can't load text data for {0} at {1}",name,filePath));
		return textFile;
	}

	enum TextFieldData{
		Movement,
		Tonnage
	}
	static int GetFieldFromTextAssetInt(TextAsset asset, TextFieldData field){return int.Parse(GetFieldFromTextAsset(asset,field));}
	public static int GetFieldFromTextAssetInt(TextAsset asset, string searchTerm){return int.Parse(GetFieldFromTextAsset(asset,searchTerm));}
	static string GetFieldFromTextAsset(TextAsset asset, TextFieldData field){
		string searchterm = "";
		if(field == TextFieldData.Movement)
			searchterm = "Walk MP:";
		else if(field == TextFieldData.Tonnage)
			searchterm = "Mass:";

		if(searchterm == "")
			return "";
		return GetFieldFromTextAsset(asset, searchterm);
	}
	public static string GetFieldFromTextAsset(TextAsset asset, string searchTerm){
		string[] lines = System.Text.RegularExpressions.Regex.Split(asset.text,"\n|\r|\r\n");
		foreach(string s in lines){
			if(s.Contains(searchTerm))
				return s.Replace(searchTerm,"");
		}

		return "";
	}
}