public interface IGameServer
{
    System.Threading.Tasks.Task<ServerActionResult> TryBuyBuildingAsync(string buildingId);
    System.Threading.Tasks.Task<ServerActionResult> TryStartQuestAsync(string questId);
    System.Threading.Tasks.Task<ServerActionResult> TryCompleteQuestAsync(string questId);
}
