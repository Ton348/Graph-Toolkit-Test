using System.Text;
using Prototype.Business.Bootstrap;
using Sample.Runtime.GameData;
using TMPro;
using UnityEngine;

public sealed class QuestListUI : MonoBehaviour
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

		var sb = new StringBuilder();
		foreach (string questId in bootstrap.PlayerStateSync.ActiveQuests)
		{
			if (string.IsNullOrWhiteSpace(questId))
			{
				continue;
			}

			QuestDefinitionData definition = bootstrap.GameDataRepository.GetQuestById(questId);
			sb.AppendLine(definition != null ? definition.title : questId);
		}

		questsText.text = sb.Length > 0 ? sb.ToString() : "No active quests";
	}
}
