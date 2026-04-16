using System.Text;
using Prototype.Business.Bootstrap;
using Sample.Runtime.GameData;
using TMPro;
using UnityEngine;

public class QuestListUi : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public TMP_Text questsText;

    private void Update()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (questsText == null || bootstrap == null || bootstrap.PlayerStateSync == null || bootstrap.GameDataRepository == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (string questId in bootstrap.PlayerStateSync.ActiveQuests)
        {
            if (string.IsNullOrEmpty(questId))
            {
                continue;
            }

            QuestDefinitionData def = bootstrap.GameDataRepository.GetQuestById(questId);
            sb.AppendLine(def != null ? def.title : questId);
        }

        questsText.text = sb.Length > 0 ? sb.ToString() : "No active quests";
    }
}
