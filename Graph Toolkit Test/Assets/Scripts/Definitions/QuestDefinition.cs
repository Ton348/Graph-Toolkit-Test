using UnityEngine;

[CreateAssetMenu(menuName = "Game/Definitions/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    public string questId;
    public string title;
    [TextArea]
    public string description;
    public int rewardMoney;
}
