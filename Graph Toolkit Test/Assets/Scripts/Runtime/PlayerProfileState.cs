public class PlayerProfileState
{
    public int Money;
    public int Bargaining;
    public int Speech;
    public int Trading;
    public int Speed;
    public int Damage;
    public int Health;

    public PlayerProfileState(EconomyConfigData config)
    {
        if (config == null)
        {
            return;
        }

        Money = config.startMoney;
        Bargaining = config.baseBargaining;
        Speech = config.baseSpeech;
        Trading = config.baseTrading;
        Speed = config.baseSpeed;
        Damage = config.baseDamage;
        Health = config.baseHealth;
    }

}
