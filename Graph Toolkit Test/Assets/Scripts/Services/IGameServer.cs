public interface IGameServer
{
    System.Threading.Tasks.Task<ServerActionResult> TryGetProfileAsync();
    System.Threading.Tasks.Task<ServerActionResult> TryBuyBuildingAsync(string buildingId, QuestActionType questAction = QuestActionType.None, string questId = null);
    System.Threading.Tasks.Task<ServerActionResult> TryStartQuestAsync(string questId);
    System.Threading.Tasks.Task<ServerActionResult> TryCompleteQuestAsync(string questId);
    System.Threading.Tasks.Task<ServerActionResult> TryFailQuestAsync(string questId);
    System.Threading.Tasks.Task<ServerActionResult> TryStealAsync(int amount, bool canFail, int successChance);
    System.Threading.Tasks.Task<ServerActionResult> TryAddMoneyAsync(int amount);
    System.Threading.Tasks.Task<ServerActionResult> TrySpendMoneyAsync(int amount);
    System.Threading.Tasks.Task<ServerActionResult> TrySaveCheckpointAsync(string graphId, string checkpointId);
}
