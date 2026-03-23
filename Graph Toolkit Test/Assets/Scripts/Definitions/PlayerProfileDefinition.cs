using UnityEngine;

[CreateAssetMenu(menuName = "Game/Definitions/Player Profile Definition")]
public class PlayerProfileDefinition : ScriptableObject
{
    public int startMoney = 1000;
    public int baseBargaining = 1;
    public int baseSpeech = 1;
    public int baseSpeed = 1;
    public int baseDamage = 1;
    public int baseHealth = 100;
}
