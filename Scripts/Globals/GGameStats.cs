public static class GGameStats{
	public static readonly int WalkHeat = 1;
	public static readonly int RunHeat = 2;
	public static readonly int JumpHeatPerLane = 2;

	public static readonly int[] RangeModifier = {-1,0,2,4};

	public static readonly int[,] ToBeHitLanesMoved = {
			{0, 1, 0},
			{2, 2, 1},
			{3, 3, 2},
			{4, 5, 3},
			{6, 8, 4},
			{9, 12, 5},
			{13, int.MaxValue, 6} };
}