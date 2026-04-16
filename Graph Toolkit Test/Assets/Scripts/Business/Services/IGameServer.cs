using System.Threading.Tasks;

public interface IGameServer
{
	Task<ServerActionResult> TryGetProfileAsync();

	Task<ServerActionResult> TryBuyBuildingAsync(
		string buildingId,
		QuestActionType questAction = QuestActionType.None,
		string questId = null);

	Task<ServerActionResult> TryStartQuestAsync(string questId);
	Task<ServerActionResult> TryCompleteQuestAsync(string questId);
	Task<ServerActionResult> TryFailQuestAsync(string questId);
	Task<ServerActionResult> TryStealAsync(int amount, bool canFail, int successChance);
	Task<ServerActionResult> TryAddMoneyAsync(int amount);
	Task<ServerActionResult> TrySpendMoneyAsync(int amount);
	Task<ServerActionResult> TrySaveCheckpointAsync(string graphId, string checkpointId);
	Task<ServerActionResult> TrySubmitTradeOfferAsync(string buildingId, int offeredAmount);
	Task<ServerActionResult> TryRentBusinessAsync(string lotId);
	Task<ServerActionResult> TryAssignBusinessTypeAsync(string lotId, string businessTypeId);
	Task<ServerActionResult> TryInstallBusinessModuleAsync(string lotId, string moduleId);
	Task<ServerActionResult> TryAssignSupplierAsync(string lotId, string supplierId);
	Task<ServerActionResult> TryHireBusinessWorkerAsync(string lotId, string roleId, string contactId);
	Task<ServerActionResult> TryOpenBusinessAsync(string lotId);
	Task<ServerActionResult> TryCloseBusinessAsync(string lotId);
	Task<ServerActionResult> TrySetBusinessMarkupAsync(string lotId, int markupPercent);
	Task<ServerActionResult> TryUnlockContactAsync(string contactId);
	Task<ServerActionResult> TryAddBusinessStockAsync(string lotId, int amount);
	Task<ServerActionResult> TryAddBusinessShelfStockAsync(string lotId, int amount);
	Task<ServerActionResult> TryClearBusinessStockAsync(string lotId);
	Task<ServerActionResult> TryResetBusinessesAsync();
	Task<ServerActionResult> TryConstructSiteVisualAsync(string siteId, string visualId);
	Task<ServerActionResult> TryRemoveSiteVisualAsync(string siteId);
}