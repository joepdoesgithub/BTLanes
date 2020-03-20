using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTCombatPreviewTextManager : MonoBehaviour{
	public Text text;

	int shooterLaneNum, targetLaneNum;
	int[] ranges;

    // Update is called once per frame
    void Update(){
		shooterLaneNum = GRefs.battleUnitManager.GetSelectedUnitLaneNum();
		targetLaneNum = GRefs.battleUnitManager.GetSelectedEnemyLaneNum();
		// ranges = GRefs.battleUnitManager.

        // if(unitSelected && targetSelected){
		// 	if(inRange? && inArc?)
		// }
		text.text = string.Format("I'm in {0} enemy in {1}",shooterLaneNum,targetLaneNum);
    }
}
