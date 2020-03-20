using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTPhaseIndicatorManager : MonoBehaviour{
	static Color32[] boxColors = {new Color32(120,120,120,255), new Color32(80,80,80,255)};
	static Color32[] textColors = {new Color32(255,255,255,255),new Color32(0,0,0,255)};
	
	public GameObject[] boxes;
	Image[] images;
	public Text[] texts;
	string battleStateRef;

	void Start(){
		images = new Image[boxes.Length];
		for(int i = 0;i<boxes.Length;i++)
			images[i] = boxes[i].GetComponent<Image>();
	}

    // Update is called once per frame
    void Update(){
        battleStateRef = Globals.GetBattleState().ToString();
		int index = -1;
		if(battleStateRef.Contains("Moving"))
			index = 0;
		else if(battleStateRef.Contains("Shooting"))
			index = 1;
		else if(battleStateRef.Contains("Physical"))
			index = 2;
		for(int i = 0;i<images.Length;i++){
			if(i==index){
				images[i].color = boxColors[0];
				texts[i].color = textColors[0];
			}else{
				images[i].color = boxColors[1];
				texts[i].color = textColors[1];
			}
		}
    }
}
