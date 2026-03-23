public class PlayerProfileState
{
    public int Money;
    public int Bargaining;
    public int Speech;
    public int Speed;
    public int Damage;
    public int Health;

    public PlayerProfileState(PlayerProfileDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        Money = definition.startMoney;
        Bargaining = definition.baseBargaining;
        Speech = definition.baseSpeech;
        Speed = definition.baseSpeed;
        Damage = definition.baseDamage;
        Health = definition.baseHealth;
    }
}
