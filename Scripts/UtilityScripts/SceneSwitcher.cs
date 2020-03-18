using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class SceneSwitcher : MonoBehaviour{
	public void SwitchToStratMap(){
		SceneManager.LoadScene("StrategicMapScene",LoadSceneMode.Single);
	}
	public void SwitchToBase(){
		SceneManager.LoadScene("MainBaseScene",LoadSceneMode.Single);
	}
	public void SwitchToGameConfig(){
		SceneManager.LoadScene("GameConfigScene",LoadSceneMode.Single);
	}
	public void SwitchToBattle(){
		TacBattleData.InitiateBattleData(Map.MapTerrain.Forest,
				new Lance[]{GLancesAndUnits.lances[0],GLancesAndUnits.lances[1]});
		SceneManager.LoadScene("BattleScene",LoadSceneMode.Single);
	}
	public void StartTheGame(){
		// Take input of config
		UnitsLoader.InitUnits(
				new string[]{
					"Hunchback HBK-4P",
					"Wolverine WVR-6M",
					"Catapult CPLT-C1",
					"Locust LCT-1E"
				},
				0);	
		UnitsLoader.InitUnits(
				new string[]{
					"Hunchback HBK-4G",
					"Stalker STK-4N"
				},
				1);

		Map.GenerateNewMap();

		// TestFunc();

		// Switch to base
		SwitchToBase();
	}

	void TestFunc(){
		foreach(KeyValuePair<GEnums.EMechLocation,int> v in GLancesAndUnits.units[0].DStructucePoints){
			Debug.Log(v.Key.ToString() + " " + v.Value.ToString());
		}

	}
}
