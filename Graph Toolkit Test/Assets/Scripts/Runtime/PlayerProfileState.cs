public class PlayerProfileState
{
    public int money;
    public int bargaining;
    public int speech;
    public int trading;
    public int speed;
    public int damage;
    public int health;

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
