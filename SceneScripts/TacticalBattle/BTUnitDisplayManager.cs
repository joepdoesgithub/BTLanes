﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTUnitDisplayManager : MonoBehaviour{
	public Text textLeft;
	public Text textRight;

	Unit dispUnitLeft = null;
	SWeaponLineWithID[] leftWeapons;
	Unit dispUnitRight = null;
	SWeaponLineWithID[] rightWeapons;
	public int GetTargetedUnitID(){return (dispUnitRight==null ? -1: dispUnitRight.ID);}

	int selectedWeaponID;

    // Start is called before the first frame update
    void Start(){
        GRefs.btUnitDisplayManager = this;
		textLeft.text = "";
		textRight.text = "";
    }

    // Update is called once per frame
    void Update(){
		string state = Globals.GetBattleState().ToString();
        if(dispUnitLeft == null)
			textLeft.text = "";
		else{
			DisplayFriendlyUnit();
		}
		if(dispUnitRight == null)
			textRight.text = "";
		else
			DisplayEnemyUnit();
    }

	public void SelectEnemyMech(){
		BattleManager.SPlayerOrder[] orders = GRefs.battleManager.GetPlayerOrders();
		bool getNext = (dispUnitRight == null ? true : false);
		for(int i = 0;;){
			if(getNext){
				if(orders[i].unit.team != 0){
					dispUnitRight = orders[i].unit;
					CreateWeaponTable(ref rightWeapons,dispUnitRight);
					break;
				}
			}else if(dispUnitRight!=null && orders[i].unit.ID == dispUnitRight.ID)
				getNext = true;

			if(i >= orders.Length - 1)
				i = 0;
			else
				i++;
		}
	}
	public void SelectFriendlyMech(Unit unit){dispUnitLeft = unit;CreateWeaponTable(ref leftWeapons,unit);selectedWeaponID = leftWeapons[0].weapon.ID;}
	public void ResetSelections(bool resetLeft, bool resetRight){
		if(resetLeft){
			dispUnitLeft = null;
			leftWeapons = null;
		}
		if(resetRight){
			dispUnitRight = null;
			rightWeapons = null;
		}
	}

	void CreateWeaponTable(ref SWeaponLineWithID[] table, Unit unit){
		GEnums.SWeapon[] sortedWeapon = GetSortedWeapons(unit);
		table = new SWeaponLineWithID[sortedWeapon.Length];
		for(int i = 0;i<leftWeapons.Length;i++){
			table[i].ID = sortedWeapon[i].ID;
			table[i].weapon = sortedWeapon[i];
		}
	}

	void DisplayFriendlyUnit(){
		string phase = Globals.GetBattleState().ToString();
		bool mvmnt = (phase.Contains("Moving")?true:false);
		bool wpnsColors = (phase.Contains("Shooting")?true:false);

		string s = dispUnitLeft.unitName + "\n";
		s += string.Format("WK: {0}",dispUnitLeft.walkSpeed);
		if(mvmnt){
			int i = GRefs.battleUnitManager.GetMoveRemaining();
			s += string.Format("\tRem: {0}", (i<0?0:i) );
		}
		s += string.Format("\nRN: {0}",dispUnitLeft.runSpeed);
		if(mvmnt)
			s += string.Format("\tRem: {0}",GRefs.battleUnitManager.GetRunRemaining());
		s += string.Format("\nHt: {0}\n\n",dispUnitLeft.heat);
		// [TODO]: show heatsinking

		// Weapons
		string[] weaponTable = GetWeaponsInStringTable(leftWeapons,selectedWeaponID,false);
		foreach(string ss in weaponTable)
			s+= ss + "\n";
		s += "\n";
		// Armour
		string[] armS = GetArmourInStringTable(dispUnitLeft);
		foreach(string ss in armS)
			s+= ss + "\n";

		textLeft.text = s;
	}

	void DisplayEnemyUnit(){
		string s = dispUnitRight.unitName + "\n";
		s += string.Format("WK: {0}",dispUnitRight.walkSpeed);
		s += string.Format("\nRN: {0}",dispUnitRight.runSpeed);
		s += string.Format("\nHt: {0}\n\n",dispUnitRight.heat);

		// Weapons
		GEnums.SWeapon[] wpnsSorted = GetSortedWeapons(dispUnitRight);
		SWeaponLineWithID[] newWpns = new SWeaponLineWithID[wpnsSorted.Length];
		for(int i = 0;i<wpnsSorted.Length;i++){
			newWpns[i].ID = wpnsSorted[i].ID;
			newWpns[i].weapon = wpnsSorted[i];
		}
		string[] weaponTable = GetWeaponsInStringTable( newWpns );
		foreach(string ss in weaponTable)
			s+= ss + "\n";
		s += "\n";
		// Armour
		string[] armS = GetArmourInStringTable(dispUnitRight);
		foreach(string ss in armS)
			s+= ss + "\n";

		textRight.text = s;
	}

	int Roundup(double x){return (int)(x+0.5);}

	GEnums.SWeapon[] GetSortedWeapons(Unit u){
		List<GEnums.SWeapon> lwpns = new List<GEnums.SWeapon>();
		foreach(GEnums.SWeapon w in u.weapons)
			lwpns.Add(w);
		GEnums.SWeapon[] wpns = new GEnums.SWeapon[lwpns.Count];
		int index = 0;
		while(lwpns.Count > 0){
			float maxDmg = int.MinValue;int ii = 0;
			for(int i = 0;i<lwpns.Count;i++){
				if(lwpns[i].GetDamage() > maxDmg){
					maxDmg = lwpns[i].GetDamage();
					ii = i;
				}
			}
			wpns[index] = lwpns[ii];
			index++;
			lwpns.RemoveAt(ii);
		}
		return wpns;
	}

	string[] GetWeaponsInStringTable(SWeaponLineWithID[] wpns, int selectedWeaponID = -1, bool enemyDisplay = true){
		string[] ss = new string[wpns.Length + 1];

		string header = "Name,HT,DMG,MnR,ShR,MdR,LnR,Loc";
			
		/////
		// Finding the correct padding
		/////
		int[] maxLens = new int[header.Split(',').Length];
		for(int i = 0;i<maxLens.Length;i++){maxLens[i]=0;}
		foreach(SWeaponLineWithID w in wpns){
			if(w.weapon.name.Length > maxLens[0])
				maxLens[0] = w.weapon.name.Length;
			if(w.weapon.heat.ToString().Length > maxLens[1])
				maxLens[1] = w.weapon.heat.ToString().Length;
			if(w.weapon.GetDamage().ToString().Length > maxLens[2])
				maxLens[2] = w.weapon.GetDamage().ToString().Length;
			if(w.weapon.ranges[0].ToString().Length > maxLens[3])
				maxLens[3] = w.weapon.ranges[0].ToString().Length;
			if(w.weapon.ranges[1].ToString().Length > maxLens[4])
				maxLens[4] = w.weapon.ranges[1].ToString().Length;
			if(w.weapon.ranges[2].ToString().Length > maxLens[5])
				maxLens[5] = w.weapon.ranges[2].ToString().Length;
			if(w.weapon.ranges[3].ToString().Length > maxLens[6])
				maxLens[6] = w.weapon.ranges[3].ToString().Length;
			if(w.weapon.loc.ToString().Length > maxLens[7])
				maxLens[7] = w.weapon.loc.ToString().Length;
		}
		for(int i = 0;i<header.Split(',').Length;i++){
			if(header.Split(',')[i].Length > maxLens[i])
				maxLens[i] = header.Split(',')[i].Length;
		}

		/////
		//	Printing the actual table
		/////
		int spacing = 2;
		bool doColors = false;
		if( (!enemyDisplay) && Globals.GetBattleState().ToString().Contains("Shooting"))
			doColors = true;
		string colorString = "";
		for(int i = 0;i<wpns.Length;i++){
			string s = "";
			bool highlightWpn = (wpns[i].ID == selectedWeaponID);
			if(highlightWpn && (!enemyDisplay))
				s+= "<b><i>";
			if(doColors){
				int colorIndex = (GRefs.battleUnitManager.HasWeaponFired( wpns[i].ID )?1:0);
				colorString = ColorUtility.ToHtmlStringRGBA(Globals.UnitDisplayColors[0,colorIndex]);
				s+="<color=#" + colorString + ">";
			}
			s += wpns[i].weapon.name.PadRight(maxLens[0] + spacing);
			s += wpns[i].weapon.heat.ToString().PadLeft(maxLens[1] + spacing);
			s += wpns[i].weapon.GetDamage().ToString().PadLeft(maxLens[2] + spacing);
			s += (Roundup(wpns[i].weapon.ranges[0]/2.0)).ToString().PadLeft(maxLens[3] + spacing);
			s += (Roundup(wpns[i].weapon.ranges[1]/2.0)).ToString().PadLeft(maxLens[4] + spacing);
			s += (Roundup(wpns[i].weapon.ranges[2]/2.0)).ToString().PadLeft(maxLens[5] + spacing);
			s += (Roundup(wpns[i].weapon.ranges[3]/2.0)).ToString().PadLeft(maxLens[6] + spacing);
			s += wpns[i].weapon.loc.ToString().PadLeft(maxLens[7]+spacing);
			if(doColors)
				s+="</color>";
			if(highlightWpn && (!enemyDisplay))
				s+= "</i></b>";
			ss[i+1] = s;
		}

		string s2 = header.Split(',')[0].PadRight(maxLens[0] + spacing);
		for(int i = 1;i<maxLens.Length;i++){
			int mySpace = spacing;		//(i!=maxLens.Length -1 ? spacing : 0);
			s2 += header.Split(',')[i].PadLeft(maxLens[i] + mySpace);
		}
		ss[0] = s2;
		return ss;
	}

	string[] GetArmourInStringTable(Unit u){
		string[] tmplt  = new string[]{
			"Armour|Structure:",
			"       HD|XX",
			"  RT|XX    LT|XX     RTL RTR",
			"RA|XX CT|XX LA|XX      RTC",    
			"",
			"   RL|XX  LL|XX"
		};
		GEnums.EMechLocation[] order = new GEnums.EMechLocation[]{
			GEnums.EMechLocation.HD,
			GEnums.EMechLocation.RT, GEnums.EMechLocation.LT,GEnums.EMechLocation.RTL,GEnums.EMechLocation.RTR,
			GEnums.EMechLocation.RA, GEnums.EMechLocation.CT,GEnums.EMechLocation.LA,GEnums.EMechLocation.RTC,
			GEnums.EMechLocation.RL, GEnums.EMechLocation.LL
		};
		string[] newTmplt = new string[tmplt.Length];
		for(int i = 0;i<tmplt.Length;i++){
			string s = tmplt[i];
			for(int j = 0;j<order.Length;j++){
				string searchTerm = order[j].ToString();
				if(j != 3 && j!= 4 && j!= 8)
				 	searchTerm += "|XX";
				if(s.Contains(searchTerm)){
					int armour = u.DArmourPoints[ order[j] ];
					string disp = armour.ToString().PadLeft(3);
					if(j != 3 && j!= 4 && j!= 8){
						int structure = u.DStructucePoints[ order[j] ];
						disp += "|" + structure.ToString().PadRight(2);
					}
					s = s.Replace(searchTerm,disp);
				}
			}
			newTmplt[i] = s;
		}
		return newTmplt;
	}

	///////////////////////////////
	///
	///			Weapons fire
	///
	///////////////////////////////

	public int GetSelectedWeaponID(int unitID){
		if(dispUnitLeft.ID != unitID)
			return -1;
		return selectedWeaponID;
	}

	public void TabWeapon(){
		if(dispUnitLeft == null)
			return;
		for(int i = 0;i<leftWeapons.Length;i++){
			if(selectedWeaponID == leftWeapons[i].ID){
				selectedWeaponID = leftWeapons[0].ID;
				if(i+1 < leftWeapons.Length)
					selectedWeaponID = leftWeapons[i+1].ID;
				break;
			}
		}
	}

	struct SWeaponLineWithID{
		public int ID;
		public GEnums.SWeapon weapon;
	}
}
