using System.Collections.Generic;
using UnityEngine;

public static class ConditionEvaluator
{
    static readonly HashSet<string> LoggedFalseNodes = new HashSet<string>();

    public static bool EvaluateCondition(
        ConditionType conditionType,
        GameRuntimeState runtimeState,
        PlayerService playerService,
        QuestService questService,
        BuildingDefinition targetBuilding,
        int requiredMoney,
        PlayerStatType playerStatType,
        int requiredStatValue)
    {
        return EvaluateCondition(conditionType, runtimeState, playerService, questService, targetBuilding, requiredMoney, playerStatType, requiredStatValue, null, out _);
    }

    static bool EvaluateCondition(
        ConditionType conditionType,
        GameRuntimeState runtimeState,
        PlayerService playerService,
        QuestService questService,
        BuildingDefinition targetBuilding,
        int requiredMoney,
        PlayerStatType playerStatType,
        int requiredStatValue,
        string questId,
        out string reason)
    {
        reason = string.Empty;

        switch (conditionType)
        {
            case ConditionType.BuildingOwned:
                return IsBuildingOwned(runtimeState, targetBuilding, out reason);
            case ConditionType.HasEnoughMoney:
                return HasEnoughMoney(runtimeState, playerService, requiredMoney, out reason);
            case ConditionType.PlayerStatAtLeast:
                return IsPlayerStatAtLeast(runtimeState, playerStatType, requiredStatValue, out reason);
            case ConditionType.QuestActive:
                return IsQuestStatus(runtimeState, questService, questId, QuestStatus.Active, out reason);
            case ConditionType.QuestCompleted:
                return IsQuestStatus(runtimeState, questService, questId, QuestStatus.Completed, out reason);
        }

        reason = $"Unknown condition type: {conditionType}";
        return false;
    }

    public static bool EvaluateCondition(ConditionNode node, GameRuntimeState runtimeState, PlayerService playerService, QuestService questService)
    {
        if (node == null)
        {
            return false;
        }

        bool result = EvaluateCondition(node.conditionType, runtimeState, playerService, questService, node.targetBuilding, node.requiredMoney, node.playerStatType, node.requiredStatValue, node.questId, out string reason);
        LogIfFalse(node, result, reason);
        return result;
    }

    public static bool EvaluateCondition(WaitForConditionNode node, GameRuntimeState runtimeState, PlayerService playerService, QuestService questService)
    {
        if (node == null)
        {
            return false;
        }

        bool result = EvaluateCondition(node.conditionType, runtimeState, playerService, questService, node.targetBuilding, node.requiredMoney, node.playerStatType, node.requiredStatValue, node.questId, out string reason);
        LogIfFalse(node, result, reason);
        return result;
    }

    static bool IsBuildingOwned(GameRuntimeState runtimeState, BuildingDefinition targetBuilding, out string reason)
    {
        if (runtimeState == null)
        {
            reason = "RuntimeState is null.";
            return false;
        }

        if (runtimeState.Buildings == null)
        {
            reason = "RuntimeState.Buildings is null.";
            return false;
        }

        if (targetBuilding == null)
        {
            reason = "Target building is not assigned.";
            return false;
        }

        foreach (var building in runtimeState.Buildings)
        {
            if (building == null || building.Definition == null)
            {
                continue;
            }

            if (building.Definition == targetBuilding)
            {
                if (building.IsOwned)
                {
                    reason = string.Empty;
                    return true;
                }

                reason = $"Building '{targetBuilding.name}' is found but not owned.";
                return false;
            }
        }

        reason = $"Building '{targetBuilding.name}' not found in runtime state.";
        return false;
    }

    static bool HasEnoughMoney(GameRuntimeState runtimeState, PlayerService playerService, int requiredMoney, out string reason)
    {
        bool hasEnough;
        if (playerService != null)
        {
            hasEnough = playerService.HasEnoughMoney(requiredMoney);
            int currentMoney = runtimeState != null && runtimeState.Player != null ? runtimeState.Player.Money : -1;
            if (!hasEnough)
            {
                reason = currentMoney >= 0
                    ? $"Money {currentMoney} < required {requiredMoney}."
                    : $"PlayerService.HasEnoughMoney({requiredMoney}) returned false.";
            }
            else
            {
                reason = string.Empty;
            }
            return hasEnough;
        }

        int money = runtimeState != null && runtimeState.Player != null ? runtimeState.Player.Money : 0;
        if (money >= requiredMoney)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Money {money} < required {requiredMoney}.";
        return false;
    }

    static bool IsPlayerStatAtLeast(GameRuntimeState runtimeState, PlayerStatType statType, int requiredStatValue, out string reason)
    {
        int value = GetPlayerStat(runtimeState, statType);
        if (value >= requiredStatValue)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Stat {statType} value {value} < required {requiredStatValue}.";
        return false;
    }

    static bool IsQuestStatus(GameRuntimeState runtimeState, QuestService questService, string questId, QuestStatus status, out string reason)
    {
        if (string.IsNullOrEmpty(questId))
        {
            reason = "Quest id is not assigned.";
            return false;
        }

        QuestState quest = questService != null ? questService.GetQuestById(questId) : GetQuestFromRuntime(runtimeState, questId);
        if (quest == null)
        {
            reason = $"Quest '{questId}' not found.";
            return false;
        }

        if (quest.Status == status)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Quest '{questId}' status is {quest.Status}, expected {status}.";
        return false;
    }

    static QuestState GetQuestFromRuntime(GameRuntimeState runtimeState, string questId)
    {
        if (runtimeState == null || runtimeState.Quests == null || string.IsNullOrEmpty(questId))
        {
            return null;
        }

        foreach (QuestState quest in runtimeState.Quests)
        {
            if (quest != null && quest.Definition != null && quest.Definition.questId == questId)
            {
                return quest;
            }
        }

        return null;
    }

    static int GetPlayerStat(GameRuntimeState runtimeState, PlayerStatType statType)
    {
        if (runtimeState == null || runtimeState.Player == null)
        {
            return 0;
        }

        var player = runtimeState.Player;
        switch (statType)
        {
            case PlayerStatType.Bargaining:
                return player.Bargaining;
            case PlayerStatType.Speech:
                return player.Speech;
            case PlayerStatType.Speed:
                return player.Speed;
            case PlayerStatType.Damage:
                return player.Damage;
            case PlayerStatType.Health:
                return player.Health;
        }

        return 0;
    }

    static void LogIfFalse(BusinessQuestNode node, bool result, string reason)
    {
        if (node == null)
        {
            return;
        }

        if (result)
        {
            if (!string.IsNullOrEmpty(node.id))
            {
                LoggedFalseNodes.Remove(node.id);
            }
            return;
        }

        if (string.IsNullOrEmpty(node.id))
        {
            Debug.LogWarning($"[ConditionEvaluator] Condition failed (no node id). {reason}");
            return;
        }

        if (LoggedFalseNodes.Add(node.id))
        {
            Debug.LogWarning($"[ConditionEvaluator] {node.GetType().Name} failed: {reason}");
        }
    }
}
