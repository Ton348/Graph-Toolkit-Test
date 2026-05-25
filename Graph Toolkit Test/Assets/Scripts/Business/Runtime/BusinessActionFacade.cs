using System;
using System.Threading.Tasks;
using Prototype.Business.Services;
using Sample.Runtime.Services;

namespace Prototype.Business.Runtime
{
	public class BusinessActionFacade
	{
		private readonly IGameServer m_gameServer;
		private readonly ProfileSyncService m_profileSync;
		private readonly RequestManager m_requestManager;

		public BusinessActionFacade(IGameServer gameServer, ProfileSyncService profileSync, RequestManager requestManager)
		{
			m_gameServer = gameServer;
			m_profileSync = profileSync;
			m_requestManager = requestManager;
		}

		public Task<ServerActionResult> RentBusiness(string lotId)
		{
			return ExecuteAsync("RentBusiness", () => m_gameServer.TryRentBusinessAsync(lotId));
		}

		public Task<ServerActionResult> AssignBusinessType(string lotId, string businessTypeId)
		{
			return ExecuteAsync("AssignBusinessType", () => m_gameServer.TryAssignBusinessTypeAsync(lotId, businessTypeId));
		}

		public Task<ServerActionResult> InstallModule(string lotId, string moduleId)
		{
			return ExecuteAsync("InstallBusinessModule", () => m_gameServer.TryInstallBusinessModuleAsync(lotId, moduleId));
		}

		public Task<ServerActionResult> SetBusinessEquipment(
			string lotId,
			string storageItemId,
			string cashDeskItemId,
			string shelfItemId)
		{
			return ExecuteAsync("SetBusinessEquipment",
				() => m_gameServer.TrySetBusinessEquipmentAsync(lotId, storageItemId, cashDeskItemId, shelfItemId));
		}

		public Task<ServerActionResult> AssignSupplier(string lotId, string supplierId)
		{
			return ExecuteAsync("AssignSupplier", () => m_gameServer.TryAssignSupplierAsync(lotId, supplierId));
		}

		public Task<ServerActionResult> ClearSupplier(string lotId)
		{
			return ExecuteAsync("ClearSupplier", () => m_gameServer.TryAssignSupplierAsync(lotId, string.Empty));
		}

		public Task<ServerActionResult> HireWorker(string lotId, string roleId, string contactId)
		{
			return ExecuteAsync("HireBusinessWorker",
				() => m_gameServer.TryHireBusinessWorkerAsync(lotId, roleId, contactId));
		}

		public Task<ServerActionResult> ClearWorker(string lotId, string roleId)
		{
			return ExecuteAsync("ClearBusinessWorker",
				() => m_gameServer.TryHireBusinessWorkerAsync(lotId, roleId, string.Empty));
		}

		public Task<ServerActionResult> OpenBusiness(string lotId)
		{
			return ExecuteAsync("OpenBusiness", () => m_gameServer.TryOpenBusinessAsync(lotId));
		}

		public Task<ServerActionResult> CloseBusiness(string lotId)
		{
			return ExecuteAsync("CloseBusiness", () => m_gameServer.TryCloseBusinessAsync(lotId));
		}

		public Task<ServerActionResult> SetMarkup(string lotId, int markupPercent)
		{
			return ExecuteAsync("SetBusinessMarkup", () => m_gameServer.TrySetBusinessMarkupAsync(lotId, markupPercent));
		}

		public Task<ServerActionResult> SetAutoDelivery(string lotId, int dailyAmount)
		{
			return ExecuteAsync("SetBusinessAutoDelivery",
				() => m_gameServer.TrySetBusinessAutoDeliveryAsync(lotId, dailyAmount));
		}

		public Task<ServerActionResult> SimulateBusinessDay(string lotId)
		{
			return ExecuteAsync("SimulateBusinessDay", () => m_gameServer.TrySimulateBusinessDayAsync(lotId));
		}

		public Task<ServerActionResult> CollectBusinessProfit(string lotId)
		{
			return ExecuteAsync("CollectBusinessProfit", () => m_gameServer.TryCollectBusinessProfitAsync(lotId));
		}

		public Task<ServerActionResult> ApplyBusinessDelivery(string lotId, int requestedAmount)
		{
			return ExecuteAsync("ApplyBusinessDelivery",
				() => m_gameServer.TryApplyBusinessDeliveryAsync(lotId, requestedAmount));
		}

		public Task<ServerActionResult> UnlockContact(string contactId)
		{
			return ExecuteAsync("UnlockContact", () => m_gameServer.TryUnlockContactAsync(contactId));
		}

		public Task<ServerActionResult> BuyItem(string traderId, string itemId)
		{
			return ExecuteAsync("BuyItem", () => m_gameServer.TryBuyItemAsync(traderId, itemId));
		}

		public Task<TraderItemsResponse> GetTraderItems(string traderId)
		{
			return m_gameServer.TryGetTraderItemsAsync(traderId);
		}

		public Task<ServerActionResult> MoveBusinessStockToShelf(string lotId, int amount)
		{
			return ExecuteAsync("MoveBusinessStockToShelf", () => m_gameServer.TryMoveBusinessStockToShelfAsync(lotId, amount));
		}

		public Task<ServerActionResult> ResetBusinesses()
		{
			return ExecuteAsync("ResetBusinesses", () => m_gameServer.TryResetBusinessesAsync());
		}

		private async Task<ServerActionResult> ExecuteAsync(string label, Func<Task<ServerActionResult>> action)
		{
			if (m_gameServer == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ServerMissing",
					"IGameServer is not available.");
			}

			if (m_requestManager != null && !m_requestManager.TryStartRequest(label))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RequestBlocked",
					"Another request is in progress.");
			}

			try
			{
				BusinessDebugLog.Log($"[BusinessServer] {label} START");
				ServerActionResult result = await action();
				if (result?.ProfileSnapshot != null)
				{
					m_profileSync?.ApplySnapshot(result.ProfileSnapshot);
				}

				BusinessDebugLog.Log(
					$"[BusinessServer] {label} Result: {(result != null && result.Success ? "Success" : "Fail")}");
				return result;
			}
			catch (Exception ex)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "Exception", ex.Message);
			}
			finally
			{
				m_requestManager?.FinishRequest();
			}
		}
	}
}
