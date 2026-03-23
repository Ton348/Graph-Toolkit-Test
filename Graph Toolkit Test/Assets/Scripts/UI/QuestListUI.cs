using System.Text;
using TMPro;
using UnityEngine;

public class QuestListUI : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public TMP_Text questsText;

    private void Update()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (questsText == null || bootstrap == null || bootstrap.RuntimeState == null || bootstrap.RuntimeState.Quests == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (QuestState quest in bootstrap.RuntimeState.Quests)
        {
            if (quest == null || quest.Definition == null)
            {
                continue;
            }

            if (quest.Status == QuestStatus.Active)
            {
                sb.AppendLine(quest.Definition.title);
            }
        }

        questsText.text = sb.Length > 0 ? sb.ToString() : "No active quests";
    }
}
