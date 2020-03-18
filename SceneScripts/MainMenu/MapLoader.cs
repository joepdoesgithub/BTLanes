using UnityEngine;
using System;
using System.Collections.Generic;

public static class MapLoader{
	public static void LoadRandomMap(ref Map.MapTile[,] map){
		System.Random rnd = new System.Random();
		UnityEngine.Object[] maps = Resources.LoadAll("Maps",typeof(TextAsset));
		int mapNum = rnd.Next( maps.Length );

		LoadMap(maps[mapNum].name,ref map);
	}
	public static void LoadMap(string name, ref Map.MapTile[,] map){
		TextAsset mapText = Resources.Load<TextAsset>("Maps/" + name);
		string[] lines = System.Text.RegularExpressions.Regex.Split(mapText.text,"\n|\r|\r\n");

		bool doRead = false;
		int sizeX = -1;
		int sizeY = 0;
		for(int i = 0;i<lines.Length;i++){
			if(lines[i] == "[!TERRAIN]")
				break;
			if(doRead){
				int x = lines[i].Split(',').Length;
				if(sizeX > 0 && sizeX != x)
					Debug.LogError(string.Format("MapLoader.Loadmap: Inconsistent map file in |{0}|",mapText.name));
				else
					sizeX = x;
				sizeY++;
			}
			if(lines[i] == "[TERRAIN]")
				doRead = true;
		}

		map = new Map.MapTile[sizeX,sizeY];
		for(int y = 0;y<sizeY;y++){for(int x = 0;x<sizeX;x++)map[x,y] = new Map.MapTile();}

		LoadTerrain(lines,ref map);
		LoadEvents(lines,ref map);
	}

	// MapEventConfig
	//	array of size [sizeY]
	//	per line [MaptileEven];[F(ixed)/U(nfixed)/E(xact)];[X],...
	//	[F/U/E]: Fixed - needs to be in this line
	//			 Unfixed - is added into the randomstack at this line
	//			 Exact - next field is used X, to determine exact place of object
	static void LoadEvents(string[] lines, ref Map.MapTile[,] map){
		bool doRead = false;
		List<string> mapEventConfigList = new List<string>();
		for(int i =0;i<lines.Length;i++){
			if(lines[i] == "[!EVENTS]")
				break;
			if(doRead){
				mapEventConfigList.Add(lines[i]);
			}
			if(lines[i] == "[EVENTS]")
				doRead = true;
		}
		string[] mapEventConfig = mapEventConfigList.ToArray();

		System.Random rnd = new System.Random();
		List<Map.MaptileEvent> mapEvents = new List<Map.MaptileEvent>();

		for(int y = 0; y < map.GetLength(1); y++){
			List<Map.MaptileEvent> fixedEvents = new List<Map.MaptileEvent>();
			string[] events = new string[]{}; bool hasEvents = false;
			if(mapEventConfig[y].Length > 0 && mapEventConfig[y].Contains(";")){
				events = mapEventConfig[y].Split(',');
				hasEvents = true;
			}

			// Check for events for this line
			if(hasEvents){
				foreach(string s in events){
					string[] fields = s.Split(';');
					if(fields[1] == "E")
						map[ int.Parse(fields[2]), y ].mapEvent = GetEventFromString(fields[0]);
					else if(fields[1] == "F")
						fixedEvents.Add(GetEventFromString(fields[0]));
					else if(fields[1] == "U")
						mapEvents.Add(GetEventFromString(fields[0]));
				}
			}

			// Find available maptile for events
			List<int> toDoIndexes = new List<int>();
			for(int i = 0;i<map.GetLength(0);i++){
				if(map[i,y].mapEvent == Map.MaptileEvent.NULL)
					toDoIndexes.Add(i);
			}
			// Fill the fixed events
			while(fixedEvents.Count > 0){
				int listIndex = rnd.Next(0,toDoIndexes.Count);
				map[ toDoIndexes[listIndex], y].mapEvent = fixedEvents[0];
				toDoIndexes.RemoveAt(listIndex);
				fixedEvents.RemoveAt(0);
			}
			// Add to other events
			int toDoEvents = toDoIndexes.Count + (y == map.GetLength(1) - 1 ? 0 : 1) - mapEvents.Count;
			if(toDoEvents > 0){
				for(int i = 0;i< toDoEvents;i++ )
					mapEvents.Add(Map.MaptileEvent.None);
			}
			// Fill the rest of the line
			for(int i = 0;i<toDoIndexes.Count;i++){
				int listIndex = rnd.Next(0,mapEvents.Count);
				map[ toDoIndexes[i] , y].mapEvent = mapEvents[listIndex];
				mapEvents.RemoveAt(listIndex);
			}
		}

		if(mapEvents.Count > 0)
			Debug.LogError(string.Format("Map.GenerateMapEvents: Too many arguments!!! {0}",mapEvents.Count));
	}

	static Map.MaptileEvent GetEventFromString(string sEvent){
		foreach(Map.MaptileEvent e in Enum.GetValues(typeof(Map.MaptileEvent))){
			if(sEvent == e.ToString() )
				return e;
		}
		return Map.MaptileEvent.None;
	}

	static void LoadTerrain(string[] lines, ref Map.MapTile[,] map){
		System.Random rnd = new System.Random();

		bool doRead = false;
		int y = 0;
		for(int i = 0;i<lines.Length;i++){
			if(lines[i] == "[!TERRAIN]")
				break;
			if(doRead){
				string[] fields = lines[i].Split(',');
				for(int x = 0;x<fields.Length;x++){
					if(fields[x] == "RA")
						map[x,y].terrain = (Map.MapTerrain)(rnd.Next( Enum.GetValues(typeof(Map.MapTerrain)).Length ));
					else{
						foreach(Map.MapTerrain m in Enum.GetValues(typeof(Map.MapTerrain))){
							if(m.ToString().Substring(0,2) == fields[x]){
								map[x,y].terrain = m;
								break;
							}
						}
					}
				}

				y++;
			}
			if(lines[i] == "[TERRAIN]")
				doRead = true;
		}
	}
}