using System.Collections.Generic;
using System;
using UnityEngine;

public static class Map{
	public enum MapTerrain{
		Clear,
		Forest,
		Mountains,
		Hills,
		River,		// Terrain with body of what in the middle
		NULL
	}

	public enum MaptileEvent{
		None,
		EnemyBase,
		Relic,
		Wreck,
		FriendlyBase,
		NULL
	}

	public class MapTile{
		public MaptileEvent mapEvent;
		public MapTerrain terrain;
		public float scoutingValue;
		public MapTile(){mapEvent = MaptileEvent.NULL;terrain = MapTerrain.NULL;scoutingValue = -1f;}
	}

	public static MapTile[,] mapTiles;

	public static void GenerateNewMap(){
		MapLoader.LoadRandomMap(ref mapTiles);
	}

	static void PrintEvents(){
		int sX = mapTiles.GetLength(0);
		int sY = mapTiles.GetLength(1);
		string s = "";
		for(int y = 0;y < sY; y++){
			for(int x = 0;x<sX;x++){
				if(mapTiles[x,y].mapEvent == MaptileEvent.NULL)
					s+="0";
				else
					s+=mapTiles[x,y].mapEvent.ToString().ToCharArray()[0];
				s+= " ";
			}
			s+="\n";
		}
		Debug.Log(s);
	}
}

