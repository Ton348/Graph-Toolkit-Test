public static class ConditionEvaluator
{
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
        switch (conditionType)
        {
            case ConditionType.BuildingOwned:
                return IsBuildingOwned(runtimeState, targetBuilding);
            case ConditionType.HasEnoughMoney:
                if (playerService != null)
                {
                    return playerService.HasEnoughMoney(requiredMoney);
                }
                return runtimeState != null && runtimeState.Player != null && runtimeState.Player.Money >= requiredMoney;
            case ConditionType.PlayerStatAtLeast:
                return GetPlayerStat(runtimeState, playerStatType) >= requiredStatValue;
        }

        return false;
    }

    public static bool EvaluateCondition(ConditionNode node, GameRuntimeState runtimeState, PlayerService playerService, QuestService questService)
    {
        if (node == null)
        {
            return false;
        }

        return EvaluateCondition(node.conditionType, runtimeState, playerService, questService, node.targetBuilding, node.requiredMoney, node.playerStatType, node.requiredStatValue);
    }

    public static bool EvaluateCondition(WaitForConditionNode node, GameRuntimeState runtimeState, PlayerService playerService, QuestService questService)
    {
        if (node == null)
        {
            return false;
        }

        return EvaluateCondition(node.conditionType, runtimeState, playerService, questService, node.targetBuilding, node.requiredMoney, node.playerStatType, node.requiredStatValue);
    }

    static bool IsBuildingOwned(GameRuntimeState runtimeState, BuildingDefinition targetBuilding)
    {
        if (runtimeState == null || runtimeState.Buildings == null || targetBuilding == null)
        {
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
                return building.IsOwned;
            }
        }

        return false;
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
}
