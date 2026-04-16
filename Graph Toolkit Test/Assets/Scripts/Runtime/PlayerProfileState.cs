using Sample.Runtime.GameData;

namespace Sample.Runtime.Runtime
{
	public class PlayerProfileState
	{
		public int bargaining;
		public int damage;
		public int health;
		public int money;
		public int speech;
		public int speed;
		public int trading;

		public PlayerProfileState(EconomyConfigData config)
		{
			if (config == null)
			{
				return;
			}

			money = config.startMoney;
			bargaining = config.baseBargaining;
			speech = config.baseSpeech;
			trading = config.baseTrading;
			speed = config.baseSpeed;
			damage = config.baseDamage;
			health = config.baseHealth;
		}
	}
}