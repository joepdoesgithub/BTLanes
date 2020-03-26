public static class GGameStats{
	// Heat
	public static readonly int WalkHeat = 1;
	public static readonly int RunHeat = 2;
	public static readonly int JumpHeatPerLane = 2;

	// Hit modifiers
	public static readonly int[] RangeModifier = {1,0,2,4};
	public static readonly int ArmMountedBonus = -1;
	public static readonly int[,] ToBeHitLanesMoved = {
			{0, 1, 0},
			{2, 2, 1},
			{3, 3, 2},
			{4, 5, 3},
			{6, 8, 4},
			{9, 12, 5},
			{13, int.MaxValue, 6} };
	public static readonly int AllWeaponsMaxRange = 11;

	// AI stats
	public static float MovesLostFraction = 0.1f;



	public static float GetAvgNumMissilesHit(int numMissiles){
		float score = 0f;
		for(int roll = 2;roll <= 12; roll++){
			float chance = 0f;
			for(int x = 1;x<=6;x++){
				for(int y = 1;y<=6;y++){
					if(x+y==roll)
						chance += 1f/36f;
				}
			}
			score += chance * GetExactNumMissiles(roll,numMissiles);
		}
		return score;
	}
	static int GetExactNumMissiles(int roll, int numMissiles){
		for(int col = 0;col < clusterHitTable.GetLength(1);col++){
			if(clusterHitTable[0,col] != numMissiles)
				continue;
			for(int row = 1; row < clusterHitTable.GetLength(0);row++){
				if(clusterHitTable[row,0] == roll)
					return clusterHitTable[row,col];
			}
		}
		return 0;
	}
	public static int GetRandomNumMissiles(int numMissilesInLauncher){return GetExactNumMissiles(UnityEngine.Random.Range(1,7)+UnityEngine.Random.Range(1,7),numMissilesInLauncher);}
	static int[,] clusterHitTable = {
		{-1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 30, 40},
		{2, 1, 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 6, 10, 12},
		{3, 1, 1, 2, 2, 2, 2, 3, 3, 3, 4, 5, 6, 10, 12},
		{4, 1, 1, 2, 2, 3, 3, 4, 4, 4, 5, 6, 9, 12, 18},
		{5, 1, 2, 2, 3, 3, 4, 4, 5, 6, 8, 9, 12, 18, 24},
		{6, 1, 2, 2, 3, 4, 4, 5, 5, 6, 8, 9, 12, 18, 24},
		{7, 1, 2, 3, 3, 4, 4, 5, 5, 6, 8, 9, 12, 18, 24},
		{8, 2, 2, 3, 3, 4, 4, 5, 5, 6, 8, 9, 12, 18, 24},
		{9, 2, 2, 3, 4, 5, 6, 6, 7, 8, 10, 12, 16, 24, 32},
		{10, 2, 3, 3, 4, 5, 6, 6, 7, 8, 10, 12, 16, 24, 32},
		{11, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 30, 40},
		{12, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 30, 40}};
}