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

	int GetD6(){return UnityEngine.Random.Range(1,7);}
	int Get2D6(){return GetD6() + GetD6();}

	void TestFunc(){
		int prev = Get2D6();
		int numSeq = 0;
		int cnt = 0;
		int[] dist = new int[11];
		for(int i = 0;i<dist.Length;i++)
			dist[i] = 0;
		
		int sum = 0;
		int maxLen = int.MinValue;

		int x;
		for(int i = 0;i<10000000;i++){
			x = Get2D6();
			dist[x-2]++;
			// Debug.Log(x);

			if(x == prev){
				numSeq++;
				continue;
			}else if(numSeq > 0){
				int seqLen = numSeq + 1;
					sum += seqLen;
				if(seqLen > maxLen)
					maxLen = seqLen;
					numSeq = 0;
					cnt++;
			}

			prev = x;
		}

		string s = "";
		for(int i = 0;i<dist.Length;i++)
			s += string.Format("{0}: {1}\n",i+2,dist[i]);
		Debug.Log(s);
		Debug.LogFormat("AvgSeqLen {0}, maxLen {1}",sum/((double)cnt),maxLen);
	}
}
