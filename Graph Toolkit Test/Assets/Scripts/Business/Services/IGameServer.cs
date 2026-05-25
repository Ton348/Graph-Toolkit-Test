using System.Threading.Tasks;
using GameGraph.Runtime.Quest;

namespace Prototype.Business.Services
{
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
		Task<ServerActionResult> TrySetBusinessEquipmentAsync(string lotId, string storageItemId, string cashDeskItemId,
			string shelfItemId);
		Task<ServerActionResult> TryAssignSupplierAsync(string lotId, string supplierId);
		Task<ServerActionResult> TryHireBusinessWorkerAsync(string lotId, string roleId, string contactId);
		Task<ServerActionResult> TryBuyItemAsync(string traderId, string itemId);
		Task<TraderItemsResponse> TryGetTraderItemsAsync(string traderId);
		Task<ServerActionResult> TryOpenBusinessAsync(string lotId);
		Task<ServerActionResult> TryCloseBusinessAsync(string lotId);
		Task<ServerActionResult> TrySetBusinessMarkupAsync(string lotId, int markupPercent);
		Task<ServerActionResult> TrySetBusinessAutoDeliveryAsync(string lotId, int dailyAmount);
		Task<ServerActionResult> TrySimulateBusinessDayAsync(string lotId);
		Task<ServerActionResult> TryCollectBusinessProfitAsync(string lotId);
		Task<ServerActionResult> TryConsumeNpcServiceAsync(string lotId, string serviceId, int requestedAmount);
		Task<ServerActionResult> TryApplyBusinessDeliveryAsync(string lotId, int requestedAmount);
		Task<ServerActionResult> TryUnlockContactAsync(string contactId);
		Task<ServerActionResult> TryMoveBusinessStockToShelfAsync(string lotId, int amount);
		Task<ServerActionResult> TryResetBusinessesAsync();
		Task<ServerActionResult> TryConstructSiteVisualAsync(string siteId, string visualId);
		Task<ServerActionResult> TryRemoveSiteVisualAsync(string siteId);
	}
}
