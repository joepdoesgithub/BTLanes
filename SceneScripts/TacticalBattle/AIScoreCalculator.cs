using System.Collections.Generic;

class AIScoreCalculator{
	int unitID;
	Unit unit;
	
	float[] idealWpnRange;
	
	public AIScoreCalculator(int unitID){
		this.unitID = unitID;
		unit = GLancesAndUnits.GetUnit(unitID);
	}

	public List<AIHelper.SLanePosition> GetScoredPositions(List<AIHelper.SLanePosition> positions){
		SetIdealWpnRange();
	}

	// f( range, heat, dmg, loc)
	// Per range, per weapon
	//		heat + wpnHeat <= sinking ? 1 : 0.5f
	//		range (>=+4,.2),(+2,.6),(+0,1),
	//		normalize to maxDmg
	void SetIdealWpnRange(){
		float maxDmg = 0;
		int maxRange = 0;
		foreach(GEnums.SWeapon wpn in unit.weapons){
			maxDmg = (wpn.GetDamage() > maxDmg ? wpn.GetDamage() : maxDmg);
			maxRange = (wpn.ranges[3] > maxRange ? wpn.ranges[3] : maxRange);
		}

		idealWpnRange = new float[maxRange + 1];
		for(int i = 0;i<idealWpnRange.Length;i++)
			idealWpnRange[i] = 0f;

		foreach(GEnums.SWeapon wpn in unit.weapons){
			float heatScore = (unit.heat + wpn.heat > unit.heatSinking ? 0.5f : 1f);
			float dmgScore = wpn.GetDamage() / maxDmg;
			float armScore = (wpn.IsArmMounted() ? 1f : 0.85f);

			for(int rng = 0;rng <= maxRange;rng++){
				float rngScore = 1f;
				if(wpn.ranges[0] > 0 && rng <= wpn.ranges[0]){
					rngScore = 1f - (wpn.ranges[0] - rng) * 0.2f;
					rngScore = (rngScore < 0f ? 0f : rngScore);
				}else if(rng > wpn.ranges[1] && rng <= wpn.ranges[2])
					rngScore = 0.6f;
				else if(rng > wpn.ranges[2] && rng <= wpn.ranges[3])
					rngScore = 0.2f;

				idealWpnRange[rng] += (heatScore + dmgScore + rngScore + armScore) / 4f;
			}
		}

		float maxVal = 0;
		foreach(float f in idealWpnRange)
			maxVal = (f > maxVal ? f : maxVal);
	
		for(int i = 0;i<idealWpnRange.Length;i++)
			idealWpnRange[i] = idealWpnRange[i] / maxVal;
	}
}