using System;

namespace Sample.Runtime.GameData
{
	[Serializable]
	public class EconomyConfigData
	{
		public int startMoney = 1000;
		public int baseBargaining = 1;
		public int baseSpeech = 1;
		public int baseTrading;
		public int baseSpeed = 1;
		public int baseDamage = 1;
		public int baseHealth = 100;
	}
}