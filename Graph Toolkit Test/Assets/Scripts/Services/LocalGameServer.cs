using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LocalGameServer : IGameServer
{
    private readonly GameRuntimeState runtime;
    private readonly BuildingService buildingService;
    private readonly QuestService questService;
    private readonly GameDataRepository dataRepository;
    private static readonly System.Random Random = new System.Random();

    public LocalGameServer(GameRuntimeState runtime, BuildingService buildingService, QuestService questService, GameDataRepository dataRepository)
    {
        this.runtime = runtime;
        this.buildingService = buildingService;
        this.questService = questService;
        this.dataRepository = dataRepository;
    }

    public async Task<ServerActionResult> TryGetProfileAsync()
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (runtime == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        return ServerActionResult.SuccessResult(BuildSnapshot(), "Profile fetch success.");
    }

    public async Task<ServerActionResult> TryBuyBuildingAsync(string buildingId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrEmpty(buildingId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingIdEmpty", "Building id is empty.");
        }

        if (runtime == null || runtime.Buildings == null || runtime.Player == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        BuildingState building = FindBuilding(buildingId, runtime.Buildings);
        if (building == null || building.Definition == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingNotFound", "Building not found.");
        }

        if (building.IsOwned)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingAlreadyOwned", "Building already owned.");
        }

        int cost = building.Definition.purchaseCost;
        if (runtime.Player.Money < cost)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney", "Not enough money.");
        }

        bool ok = buildingService != null && buildingService.TryBuyBuilding(building, runtime.Player);
        return ok
            ? ServerActionResult.SuccessResult(BuildSnapshot(), "Buy building success.")
            : ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuyFailed", "Buy building failed.");
    }

    public async Task<ServerActionResult> TryStartQuestAsync(string questId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrEmpty(questId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound", "Quest id is empty.");
        }

        QuestDefinitionData questDefinition = dataRepository != null ? dataRepository.GetQuestById(questId) : null;
        if (questDefinition == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound", "Quest definition not found.");
        }

        return TryStartQuestInternal(questDefinition);
    }

    public async Task<ServerActionResult> TryCompleteQuestAsync(string questId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrEmpty(questId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest id is empty.");
        }

        if (runtime == null || runtime.Quests == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        QuestState quest = questService != null ? questService.GetQuestById(questId) : null;
        if (quest == null || quest.Status != QuestStatus.Active)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest is not active.");
        }

        questService?.CompleteQuest(questId);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Complete quest success.");
    }

    public async Task<ServerActionResult> TryFailQuestAsync(string questId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrEmpty(questId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest id is empty.");
        }

        if (runtime == null || runtime.Quests == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        QuestState quest = questService != null ? questService.GetQuestById(questId) : null;
        if (quest == null || quest.Status != QuestStatus.Active)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest is not active.");
        }

        questService?.FailQuest(questId);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Fail quest success.");
    }

    public async Task<ServerActionResult> TryAddMoneyAsync(int amount)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (runtime == null || runtime.Player == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        if (amount <= 0)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidAmount", "Amount must be > 0.");
        }

        runtime.Player.Money += amount;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Add money success.");
    }

    public async Task<ServerActionResult> TrySpendMoneyAsync(int amount)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (runtime == null || runtime.Player == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        if (amount <= 0)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidAmount", "Amount must be > 0.");
        }

        if (runtime.Player.Money < amount)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney", "Not enough money.");
        }

        runtime.Player.Money -= amount;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Spend money success.");
    }

    public async Task<ServerActionResult> TryStealAsync(int amount, bool canFail, int successChance)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (runtime == null || runtime.Player == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        if (!canFail)
        {
            runtime.Player.Money += amount;
            return ServerActionResult.SuccessResult(BuildSnapshot(), "Steal success.");
        }

        int roll;
        lock (Random)
        {
            roll = Random.Next(0, 100);
        }

        bool success = roll < Mathf.Clamp(successChance, 0, 100);
        if (!success)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "StealFailed", "Steal failed.");
        }

        runtime.Player.Money += amount;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Steal success.");
    }

    private static BuildingState FindBuilding(string buildingId, List<BuildingState> buildings)
    {
        if (buildings == null) return null;

        for (int i = 0; i < buildings.Count; i++)
        {
            BuildingState state = buildings[i];
            if (state != null && state.Definition != null && state.Definition.id == buildingId)
            {
                return state;
            }
        }

        return null;
    }

    private ProfileSnapshot BuildSnapshot()
    {
        var snapshot = new ProfileSnapshot();

        if (runtime != null && runtime.Player != null)
        {
            snapshot.Money = runtime.Player.Money;
            snapshot.Bargaining = runtime.Player.Bargaining;
            snapshot.Speech = runtime.Player.Speech;
            snapshot.Speed = runtime.Player.Speed;
            snapshot.Damage = runtime.Player.Damage;
            snapshot.Health = runtime.Player.Health;
        }

        if (runtime != null && runtime.Quests != null)
        {
            foreach (QuestState quest in runtime.Quests)
            {
                if (quest == null || quest.Definition == null)
                {
                    continue;
                }

                string id = quest.Definition.id;
                if (quest.Status == QuestStatus.Active)
                {
                    snapshot.ActiveQuestIds.Add(id);
                }
                else if (quest.Status == QuestStatus.Completed)
                {
                    snapshot.CompletedQuestIds.Add(id);
                }
            }
        }

        if (runtime != null && runtime.Buildings != null)
        {
            foreach (BuildingState building in runtime.Buildings)
            {
                if (building == null || building.Definition == null)
                {
                    continue;
                }

                var buildingSnapshot = new BuildingStateSnapshot
                {
                    id = building.Definition.id,
                    owned = building.IsOwned,
                    level = building.Level,
                    currentIncome = building.CurrentIncome,
                    currentExpenses = building.CurrentExpenses
                };
                snapshot.BuildingStates.Add(buildingSnapshot);

                if (building.IsOwned)
                {
                    snapshot.OwnedBuildingIds.Add(building.Definition.id);
                }
            }
        }

        return snapshot;
    }

    private ServerActionResult TryStartQuestInternal(QuestDefinitionData questDefinition)
    {
        if (questDefinition == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound", "Quest definition is null.");
        }

        if (runtime == null || runtime.Quests == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        QuestState existing = questService != null ? questService.GetQuestById(questDefinition.id) : null;
        if (existing != null && existing.Status == QuestStatus.Active)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestAlreadyActive", "Quest already active.");
        }

        questService?.AcceptQuest(questDefinition);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Start quest success.");
    }

    private static int NextDelayMs()
    {
        lock (Random)
        {
            return Random.Next(100, 501);
        }
    }

    private static ServerActionResult.ErrorType SampleNetworkIssue()
    {
        double roll;
        lock (Random)
        {
            roll = Random.NextDouble();
        }

        if (roll < 0.05)
        {
            return ServerActionResult.ErrorType.NetworkError;
        }

        if (roll < 0.08)
        {
            return ServerActionResult.ErrorType.Timeout;
        }

        return ServerActionResult.ErrorType.None;
    }
}
