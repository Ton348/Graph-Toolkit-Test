using System.Collections.Generic;
using GameGraph.Runtime.Common;
using Graph.Core.Runtime;
using Prototype.Business.Services;
using Sample.Runtime.Runtime;

namespace Prototype.Business.Runtime
{
	public static class ConditionEvaluator
	{
		private static readonly HashSet<string> s_loggedFalseNodes = new();

		private static bool EvaluateCondition(
			ConditionType conditionType,
			string buildingId,
			int requiredMoney,
			PlayerStatType playerStatType,
			int requiredStatValue,
			string questId,
			PlayerStateSync playerStateSync,
			out string reason)
		{
			reason = string.Empty;

			switch (conditionType)
			{
				case ConditionType.BuildingOwned:
					return IsBuildingOwned(buildingId, playerStateSync, out reason);
				case ConditionType.HasEnoughMoney:
					return HasEnoughMoney(playerStateSync, requiredMoney, out reason);
				case ConditionType.PlayerStatAtLeast:
					return IsPlayerStatAtLeast(playerStateSync, playerStatType, requiredStatValue, out reason);
				case ConditionType.QuestActive:
					return IsQuestStatus(playerStateSync, questId, QuestStatus.Active, out reason);
				case ConditionType.QuestCompleted:
					return IsQuestStatus(playerStateSync, questId, QuestStatus.Completed, out reason);
			}

			reason = $"Unknown condition type: {conditionType}";
			return false;
		}

		public static bool EvaluateCondition(ConditionNode node, PlayerStateSync playerStateSync)
		{
			if (node == null)
			{
				return false;
			}

			bool result = EvaluateCondition(node.conditionType, node.buildingId, node.requiredMoney, node.playerStatType,
				node.requiredStatValue, node.questId, playerStateSync, out string reason);
			LogIfFalse(node, result, reason);
			return result;
		}

		private static bool IsBuildingOwned(string buildingId, PlayerStateSync playerStateSync, out string reason)
		{
			reason = "BuildingOwned condition is deprecated in businesses-only model.";
			return false;
		}

		private static bool HasEnoughMoney(PlayerStateSync playerStateSync, int requiredMoney, out string reason)
		{
			if (playerStateSync == null)
			{
				reason = "PlayerStateSync is null.";
				return false;
			}

			bool ok = playerStateSync.Money >= requiredMoney;
			reason = ok ? string.Empty : $"Money {playerStateSync.Money} < required {requiredMoney}.";
			return ok;
		}

		private static bool IsPlayerStatAtLeast(
			PlayerStateSync playerStateSync,
			PlayerStatType statType,
			int requiredStatValue,
			out string reason)
		{
			int value = GetPlayerStat(playerStateSync, statType);
			if (value >= requiredStatValue)
			{
				reason = string.Empty;
				return true;
			}

			reason = $"Stat {statType} value {value} < required {requiredStatValue}.";
			return false;
		}

		private static bool IsQuestStatus(
			PlayerStateSync playerStateSync,
			string questId,
			QuestStatus status,
			out string reason)
		{
			if (string.IsNullOrEmpty(questId))
			{
				reason = "Quest id is not assigned.";
				return false;
			}

			if (playerStateSync == null)
			{
				reason = "PlayerStateSync is null.";
				return false;
			}

			bool ok = status == QuestStatus.Active
				? playerStateSync.IsQuestActive(questId)
				: playerStateSync.IsQuestCompleted(questId);
			reason = ok ? string.Empty : $"Quest '{questId}' is not {status}.";
			return ok;
		}

		private static int GetPlayerStat(PlayerStateSync playerStateSync, PlayerStatType statType)
		{
			switch (statType)
			{
				case PlayerStatType.Bargaining:
					return playerStateSync != null ? playerStateSync.Bargaining : 0;
				case PlayerStatType.Speech:
					return playerStateSync != null ? playerStateSync.Speech : 0;
				case PlayerStatType.Speed:
					return playerStateSync != null ? playerStateSync.Speed : 0;
				case PlayerStatType.Damage:
					return playerStateSync != null ? playerStateSync.Damage : 0;
				case PlayerStatType.Health:
					return playerStateSync != null ? playerStateSync.Health : 0;
			}

			return 0;
		}

		private static void LogIfFalse(BaseGraphNode node, bool result, string reason)
		{
			if (node == null)
			{
				return;
			}

			if (result)
			{
				if (!string.IsNullOrEmpty(node.id))
				{
					s_loggedFalseNodes.Remove(node.id);
				}

				return;
			}

			if (string.IsNullOrEmpty(node.id))
			{
				return;
			}

			s_loggedFalseNodes.Add(node.id);
		}
	}
}
