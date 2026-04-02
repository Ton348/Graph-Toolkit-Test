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
    System.Threading.Tasks.Task<ServerActionResult> TrySubmitTradeOfferAsync(string buildingId, int offeredAmount);
    System.Threading.Tasks.Task<ServerActionResult> TryRentBusinessAsync(string lotId);
    System.Threading.Tasks.Task<ServerActionResult> TryAssignBusinessTypeAsync(string lotId, string businessTypeId);
    System.Threading.Tasks.Task<ServerActionResult> TryInstallBusinessModuleAsync(string lotId, string moduleId);
    System.Threading.Tasks.Task<ServerActionResult> TryAssignSupplierAsync(string lotId, string supplierId);
    System.Threading.Tasks.Task<ServerActionResult> TryHireBusinessWorkerAsync(string lotId, string roleId, string contactId);
    System.Threading.Tasks.Task<ServerActionResult> TryOpenBusinessAsync(string lotId);
    System.Threading.Tasks.Task<ServerActionResult> TryCloseBusinessAsync(string lotId);
    System.Threading.Tasks.Task<ServerActionResult> TrySetBusinessMarkupAsync(string lotId, int markupPercent);
    System.Threading.Tasks.Task<ServerActionResult> TryUnlockContactAsync(string contactId);
    System.Threading.Tasks.Task<ServerActionResult> TryAddBusinessStockAsync(string lotId, int amount);
    System.Threading.Tasks.Task<ServerActionResult> TryAddBusinessShelfStockAsync(string lotId, int amount);
    System.Threading.Tasks.Task<ServerActionResult> TryClearBusinessStockAsync(string lotId);
    System.Threading.Tasks.Task<ServerActionResult> TryResetBusinessesAsync();
}
