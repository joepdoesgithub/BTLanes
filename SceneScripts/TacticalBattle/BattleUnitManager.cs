using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitManager : MonoBehaviour{
	public GameObject UILaneBoxHolder;
	public GameObject UnitPrefab;
	public GameObject SelectCursorPrefab;
	public GameObject TargetCursorPrefab;

	GameObject[] lanes;
	GameObject[,] mechs;
	public int GetLaneCount(){return lanes.Length;}
	public SUnitInLane[] unitsInLanes;

	Unit selectedUnit;
	public bool unitWalkedBackwards;

	BTShootingHelper shootingHelper;

	GameObject[] selectCursor;
	float prevTime;float degPerSec = 60f;

	public class SUnitInLane{
		public int laneNum = -1;
		public int facing;
		public Sprite unitSprite = null;
		public Unit unit = null;
		public bool top;
	}

	int moveOriginalLaneNum = 0, moveOriginalFacing = 0;
	int moveRemaining, runRemaining;
	public int GetMoveRemaining(){return moveRemaining;}
	public int GetRunRemaining(){return runRemaining;}

    // Start is called before the first frame update
    void Start(){
		GRefs.battleUnitManager = this;
		selectCursor = new GameObject[2];

        lanes = new GameObject[UILaneBoxHolder.transform.childCount];
		mechs = new GameObject[lanes.Length,2];
		for(int i = 0;i<lanes.Length;i++){
			lanes[i] = GameObject.Find("Lane" + i.ToString());
			mechs[i,0] = GameObject.Find("TopMech" + i.ToString());
			mechs[i,0].SetActive(false);
			mechs[i,1] = GameObject.Find("BottomMech" + i.ToString());
			mechs[i,1].SetActive(false);

			if(mechs[i,0] == null)
				Debug.LogError("Could not find topmech " + i);
			if(mechs[i,1] == null)
				Debug.LogError("Could not find bottommech " + i);
		}
		InitiateAllUnits();
		Globals.BattleUnitManagerInitDone = true;
    }

	void InitiateAllUnits(){
		List<SUnitInLane> l = new List<SUnitInLane>();
		string sout = "Initiatives: ";
		for(int team = 0;team <=1; team++){
			sout += (team==0?"Friendly[":"Enemy:[");
			int unitsPlaced = 0;
			int counter = 0;
			for(int i = TacBattleData.lances[team].units.Length - 1; i>= 0; i--){
				Unit u = TacBattleData.lances[team].units[i];

				// Lanenum
				bool startLeft = (team == 0 ? true : false);
				int unitLaneNum = startLeft ? counter : lanes.Length - 1 - counter;
				if(team == 0)
					unitLaneNum = i;
				unitsPlaced++;if(unitsPlaced%2==0){counter++;}

				// Facing
				int facing = (team==0?1:-1);

				// Unitsprie
				Sprite unitSprite = UnitsLoader.GetUnitSprite(u);
				
				// Top
				bool top = true;
				if(unitsInLanes != null)
					top = NewUnitTopOrBot(u,unitLaneNum);

				SUnitInLane unitInLane = new SUnitInLane();
				unitInLane.laneNum = unitLaneNum;
				unitInLane.facing = facing;
				unitInLane.unitSprite = unitSprite;
				unitInLane.top = top;
				unitInLane.unit = u;
				
				l.Add(unitInLane);
				unitsInLanes = l.ToArray();

				mechs[unitInLane.laneNum,(unitInLane.top?0:1)].SetActive(true);
				mechs[unitInLane.laneNum,(unitInLane.top?0:1)].GetComponent<Image>().sprite = unitInLane.unitSprite;
				float rot = (facing<0?90:-90);
				mechs[unitInLane.laneNum,(unitInLane.top?0:1)].transform.rotation = Quaternion.Euler(0f,0f,rot);
				mechs[unitInLane.laneNum,(unitInLane.top?0:1)].GetComponent<Image>().color = Globals.UnitDisplayColors[ u.team, 0 ];

				sout += string.Format("{0},{1} ",u.unitName,u.pilot.Initiative);
			}
			sout += "]";
		}
		Debug.Log(sout);
	}

	void PlaceUnitInNewLane(int unitID, int newLane, int newFacing, bool hasActed){
		Sprite s = null;
		bool newPlaceTop = false;
		int team = -1;
		foreach(SUnitInLane u in unitsInLanes){
			if(u.unit.ID == unitID){
				s = u.unitSprite;
				mechs[u.laneNum, (u.top?0:1) ].SetActive(false);
				u.laneNum = newLane;
				u.facing = newFacing;
				u.top = NewUnitTopOrBot(u.unit,newLane);
				newPlaceTop = u.top;
				team = u.unit.team;
				break;
			}
		}
		int yIndex = (newPlaceTop ? 0 : 1);
		mechs[newLane,yIndex].SetActive(true);
		mechs[newLane,yIndex].GetComponent<Image>().sprite = s;
		float rot = (newFacing<0?90:-90);
		mechs[newLane,yIndex].transform.rotation = Quaternion.Euler(0f,0f,rot);
		mechs[newLane,yIndex].GetComponent<Image>().color = Globals.UnitDisplayColors[ team, (hasActed?1:0) ];
	}

	bool NewUnitTopOrBot(Unit unit, int laneNum){
		foreach(SUnitInLane u in unitsInLanes){
			if(u.laneNum == laneNum && u.unit.ID != unit.ID){
				return !u.top;
			}
		}
		return true;
	}

	public void TurnUnitLeft(){MoveUnit(0,-1);}
	public void TurnUnitRight(){MoveUnit(0,1);}
	public void MoveUnitRight(){MoveUnit(1);}
	public void MoveUnitLeft(){MoveUnit(-1);}
	void MoveUnit(int moveDirection = 0, int turnDirection = 0){
		if(selectedUnit == null)
			return;
		
		BTMovementHelper helper = new BTMovementHelper(selectedUnit);
		int newLane = -1;
		int facing = GetUnitFacing(selectedUnit);
		if(moveDirection != 0){
			helper.MoveUnit(moveDirection,facing,ref moveRemaining,ref runRemaining, out newLane);
			if(newLane != -1)
				PlaceUnitInNewLane(selectedUnit.ID,newLane,facing,false);
		}
		if(turnDirection != 0){
			int newFacing = facing;
			helper.TurnUnit(turnDirection,facing,ref moveRemaining,ref runRemaining,out newFacing);
			if(newFacing != facing)
				PlaceUnitInNewLane(selectedUnit.ID,GetUnitLaneNum(selectedUnit),newFacing,false);
		}	
	}
	
	public void ResetUnit(){
		if(selectedUnit == null)
			return;

		moveRemaining = selectedUnit.walkSpeed;
		runRemaining = selectedUnit.runSpeed;
		unitWalkedBackwards = false;
		PlaceUnitInNewLane(selectedUnit.ID,moveOriginalLaneNum,moveOriginalFacing,false);
	}

	public void NextPhase(){
		foreach(SUnitInLane s in unitsInLanes)
			mechs[ GetUnitLaneNum(s.unit), (GetUnitTopBot(s.unit)?0:1) ].GetComponent<Image>().color = Globals.UnitDisplayColors[ s.unit.team, 0];
		UnselectAllUnits();
	}

	void Update(){
		if(selectCursor == null || selectCursor[0] == null || selectCursor[1] == null)
			return;

		float x = Time.time - prevTime;
		prevTime = Time.time;
		selectCursor[0].transform.Rotate(0f,0f,x*degPerSec);
		selectCursor[1].transform.Rotate(0f,0f,-x*degPerSec);

		foreach(SUnitInLane u in unitsInLanes){
			if(u.unit.ID == selectedUnit.ID){					
				selectCursor[0].transform.SetParent(mechs[u.laneNum, (u.top?0:1)].transform);
				selectCursor[0].transform.localPosition = new Vector3(0f,0f,0f);
				selectCursor[1].transform.SetParent(mechs[u.laneNum, (u.top?0:1)].transform);
				selectCursor[1].transform.localPosition = new Vector3(0f,0f,0f);
				break;
			}
		}
	}
	public void FinishMove(){FinishMove(selectedUnit);}
	public void FinishMove(Unit unit){
		GRefs.battleManager.FinishCurrentActingUnit();
		mechs[ GetUnitLaneNum(unit), (GetUnitTopBot(unit)?0:1) ].GetComponent<Image>().color = Globals.UnitDisplayColors[ unit.team, 1];
		UnselectAllUnits();
	}

	public void SelectMech(Unit unit){
		UnselectAllUnits();

		for(int i = 0;i<unitsInLanes.Length;i++){
			if(unit.ID == unitsInLanes[i].unit.ID){
				GRefs.btUnitDisplayManager.ResetSelections(true,false);
				GRefs.btUnitDisplayManager.SelectFriendlyMech(unitsInLanes[i].unit);

				selectedUnit = unitsInLanes[i].unit;

				if(Globals.GetBattleState().ToString().Contains("Moving")){
					moveOriginalLaneNum = unitsInLanes[i].laneNum;
					moveOriginalFacing = unitsInLanes[i].facing;
					unitWalkedBackwards = false;
					moveRemaining = selectedUnit.walkSpeed;
					runRemaining = selectedUnit.runSpeed;
				}else if(Globals.GetBattleState().ToString().Contains("Shooting"))
					shootingHelper = new BTShootingHelper(selectedUnit);

				System.Random rnd = new System.Random();
				selectCursor = new GameObject[2];
				selectCursor[0] = Instantiate(SelectCursorPrefab, new Vector3(0f,0f,0f),Quaternion.identity);
				selectCursor[0].transform.localScale = new Vector3(0.85f,0.7f,1f);
				selectCursor[1] = Instantiate(SelectCursorPrefab, new Vector3(0f,0f,0f),Quaternion.Euler(0f,0f,(float)rnd.Next(5,45)));
				selectCursor[1].transform.localScale = new Vector3(0.7f,0.85f,1f);
				return;
			}
		}
		Debug.LogError(string.Format("BattleUnitManager.SelectMech: Could not find a mech apparantly {0} {1}",unit.unitName,unit.ID));
	}

	public void UnselectAllUnits(){
		selectedUnit = null;
		if(selectCursor != null){
			if(selectCursor[0] != null)
				Destroy(selectCursor[0]);
			if(selectCursor[1] != null)
				Destroy(selectCursor[1]);
		}
		selectCursor = null;
	}
	int GetUnitFacing(Unit unit){
		foreach(SUnitInLane u in unitsInLanes){
			if(u.unit.ID == unit.ID)
				return u.facing;
		}
		return 0;
	}
	int GetUnitLaneNum(Unit unit){
		foreach(SUnitInLane u in unitsInLanes){
			if(u.unit.ID == unit.ID)
				return u.laneNum;
		}
		return -1;
	}
	bool GetUnitTopBot(Unit unit){
		foreach(SUnitInLane u in unitsInLanes){
			if(u.unit.ID == unit.ID)
				return u.top;
		}
		return true;
	}

///////////////////////////////////////////////
/////
//					Shooting stuff
/////
///////////////////////////////////////////////
	// public int GetSelectedWeaponID(){
	// 	if(selectedUnit == null)
	// 		return -1;
	// 	return GRefs.btUnitDisplayManager.GetSelectedWeaponID(selectedUnit.ID);
	// }

	public void FinishShooting(){
		Debug.Log("TODO FinishShooting");
	}

	public void FireSelectedWeaponAtTarget(){
		if(selectedUnit==null || shootingHelper==null)
			return;
		int targetID = GRefs.btUnitDisplayManager.GetTargetedUnitID();
		if(targetID < 0){
			GlobalFuncs.PostMessage("You need a target if you wish to fire");
			return;
		}
		shootingHelper.ShootWeaponAtTargetID(GRefs.btUnitDisplayManager.GetSelectedWeaponID(selectedUnit.ID), targetID);
		GRefs.btUnitDisplayManager.TabWeapon();
	}
	public bool HasWeaponFired(int weaponID){
		if(shootingHelper==null)
			return false;
		return shootingHelper.HasWeaponFiredFromID(weaponID);
	}
}