using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTRangeBandManager : MonoBehaviour{
	public GameObject UILaneBoxHolder;
	public GameObject RangebandPrefab;

	public Sprite MinRangeSprite;
	public Sprite ShortRangeSprite;
	public Sprite MediumRangeSprite;
	public Sprite LongRangeSprite;

	GameObject[] bandsLeft;
	GameObject[] bandsRight;

	int unitID;
	int weaponID;
	int lane;
	int facing;
	int[] ranges;

	int newUnitID;
	int newWeaponID;
	int newLane;
	int newFacing;

	// lane
	// facing
	// ranges

	int laneI;

	void Start(){
		bandsLeft = new GameObject[ UILaneBoxHolder.transform.childCount ];
		bandsRight = new GameObject[ UILaneBoxHolder.transform.childCount ];
		for(int i = 0;i<bandsRight.Length;i++){
			string sLeft = string.Format("RangeBandL ({0})",i);
			string sRight = string.Format("RangeBandR ({0})",i);
			bandsLeft[i] = GameObject.Find(sLeft);
			bandsLeft[i].SetActive(false);
			if(bandsLeft[i] == null)
				Debug.LogErrorFormat("BTRangeBandManager: could not find {0}",sLeft);
			bandsRight[i] = GameObject.Find(sRight);
			bandsRight[i].SetActive(false);
			if(bandsRight[i] == null)
				Debug.LogErrorFormat("BTRangeBandManager: could not find {0}",sRight);
		}
	}

    // Update is called once per frame
    void Update(){
		GRefs.battleUnitManager.GetRangeBandInfo(out newUnitID, out newWeaponID, out newLane, out newFacing, out ranges);

		if(newUnitID == unitID && newWeaponID == weaponID && newLane == lane && newFacing == facing){
			return;
		}

		if(ranges.Length != 4 || newFacing == 0)
			return;

		ResetBands();

		unitID = newUnitID;weaponID = newWeaponID;lane = newLane;facing = newFacing;

		// MinRange
		if(ranges[0] > 0 && ranges[0] != ranges[1]){
			laneI = lane + (facing<0?-1:1) * ranges[0];
			if(facing < 0){
				bandsLeft[laneI].SetActive(true);
				bandsLeft[laneI].GetComponent<Image>().sprite = MinRangeSprite;
			}else{
				bandsRight[laneI].SetActive(true);
				bandsRight[laneI].GetComponent<Image>().sprite = MinRangeSprite;
			}
		}

		// ShortRange
		laneI = lane + (facing<0?-1:1) * ranges[1];
		if(laneI >= 0 && laneI < bandsRight.Length){
			if(facing < 0){
				bandsLeft[laneI].SetActive(true);
				bandsLeft[laneI].GetComponent<Image>().sprite = ShortRangeSprite;
			}else{
				bandsRight[laneI].SetActive(true);
				bandsRight[laneI].GetComponent<Image>().sprite = ShortRangeSprite;
			}
		}
		// MediumRange
		laneI = lane + (facing<0?-1:1) * ranges[2];
		if(laneI >= 0 && laneI < bandsRight.Length){
			if(facing < 0){
				bandsLeft[laneI].SetActive(true);
				bandsLeft[laneI].GetComponent<Image>().sprite = MediumRangeSprite;
			}else{
				bandsRight[laneI].SetActive(true);
				bandsRight[laneI].GetComponent<Image>().sprite = MediumRangeSprite;
			}
		}
		// LongRange
		laneI = lane + (facing<0?-1:1) * ranges[3];
		if(laneI >= 0 && laneI < bandsRight.Length){
			if(facing < 0){
				bandsLeft[laneI].SetActive(true);
				bandsLeft[laneI].GetComponent<Image>().sprite = LongRangeSprite;
			}else{
				bandsRight[laneI].SetActive(true);
				bandsRight[laneI].GetComponent<Image>().sprite = LongRangeSprite;
			}
		}
    }

	void ResetBands(){
		for(int i = 0;i<bandsLeft.Length;i++){
			bandsLeft[i].SetActive(false);
			bandsRight[i].SetActive(false);
		}
	}
}
