public static class GEnums{
	public enum EScreenState{
		TacticalBattle,
		Base
	}

	public enum EMechLocation{
		HD,
		CT,
		LT,
		RT,
		LA,
		RA,
		LL,
		RL,
		RTC,
		RTL,
		RTR
	}
	public enum EWeaponType{
		Energy,
		Ballistic,
		MissileLRM,
		MissileSRM
	}
	public struct SWeapon{
		public string name;
		public EWeaponType type;
		public float heat;
		public EMechLocation loc;
		// public float[] ranges;
		public int[] ranges;
		public float damage;
		public float rechargeTime;				// This is used to modify the time a recharge needs
		public float dmgPerMissile;
		public int missileGroupSize;

		public int ID;

		public float GetDamage(){
			if(type == EWeaponType.MissileLRM || type == EWeaponType.MissileSRM)
				return int.Parse(name.Split(' ')[1]) * dmgPerMissile;
			else
				return damage;
		}
	}
	public enum EBattleState{
		AllInit,
		MovingInit,
		MovingSelectNext,
		MovingWaitingForInput,
		ShootingInit,
		ShootingSelectNext,
		ShootingWaitingForInput,
		PhysicalInit,
		PhysicalSelectNext,
		PhysicalWaitingForInput,
		EndPhase
	}
}