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

		public Task<ServerActionResult> AssignSupplier(string lotId, string supplierId)
		{
			return ExecuteAsync("AssignSupplier", () => m_gameServer.TryAssignSupplierAsync(lotId, supplierId));
		}

		public Task<ServerActionResult> HireWorker(string lotId, string roleId, string contactId)
		{
			return ExecuteAsync("HireBusinessWorker",
				() => m_gameServer.TryHireBusinessWorkerAsync(lotId, roleId, contactId));
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

		public Task<ServerActionResult> UnlockContact(string contactId)
		{
			return ExecuteAsync("UnlockContact", () => m_gameServer.TryUnlockContactAsync(contactId));
		}

		public Task<ServerActionResult> AddBusinessStock(string lotId, int amount)
		{
			return ExecuteAsync("AddBusinessStock", () => m_gameServer.TryAddBusinessStockAsync(lotId, amount));
		}

		public Task<ServerActionResult> AddBusinessShelfStock(string lotId, int amount)
		{
			return ExecuteAsync("AddBusinessShelfStock", () => m_gameServer.TryAddBusinessShelfStockAsync(lotId, amount));
		}

		public Task<ServerActionResult> ClearBusinessStock(string lotId)
		{
			return ExecuteAsync("ClearBusinessStock", () => m_gameServer.TryClearBusinessStockAsync(lotId));
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