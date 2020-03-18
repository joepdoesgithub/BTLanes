using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTInitiativeOrderManager : MonoBehaviour{
	public GameObject[] InitObjects;
	Image[] images;
	GEnums.EBattleState battleStateRef;
	BattleManager.SPlayerOrder[] orders;

	Dictionary<int, Sprite> DIdSprite;
	Dictionary<int, Color32> DIdColor;

	

    // Start is called before the first frame update
    void Start(){
		images = new Image[InitObjects.Length];
        for(int i = 0;i<InitObjects.Length;i++){
			images[i] = InitObjects[i].GetComponent<Image>();
			InitObjects[i].SetActive(false);
		}

		DIdSprite = new Dictionary<int, Sprite>();
		DIdColor = new Dictionary<int, Color32>();
		List<Unit> units = TacBattleData.GetAllUnitsInBattle();
		foreach(Unit u in units){
			DIdSprite.Add(u.ID,UnitsLoader.GetUnitSprite(u));
			DIdColor.Add(u.ID, Globals.UnitDisplayColors[ u.team, 0 ]);
		}
    }

    // Update is called once per frame
    void Update(){
        battleStateRef = Globals.GetBattleState();
		if(battleStateRef == GEnums.EBattleState.MovingSelectNext || battleStateRef == GEnums.EBattleState.MovingWaitingForInput ||
					battleStateRef == GEnums.EBattleState.ShootingSelectNext || battleStateRef == GEnums.EBattleState.ShootingWaitingForInput){
			orders = GRefs.battleManager.GetPlayerOrders();
			int startI = -1;
			int placed = 0;
			for(int i = 0;i<orders.Length;i++){
				if(!orders[i].hasActed || orders[i].isActing){
					if(startI<0)
						startI = i;
					InitObjects[i-startI].SetActive(true);
					images[i-startI].sprite = DIdSprite[orders[i].unit.ID];
					images[i-startI].color = DIdColor[orders[i].unit.ID];
					placed++;
				}
			}
			for(int i = placed;i<images.Length;i++)
				InitObjects[i].SetActive(false);
		}else{
			foreach(GameObject o in InitObjects)
				o.SetActive(false);
		}
    }
}
