using System;
using System.Threading.Tasks;

public class BusinessActionFacade
{
    private readonly IGameServer gameServer;
    private readonly ProfileSyncService profileSync;
    private readonly RequestManager requestManager;

    public BusinessActionFacade(IGameServer gameServer, ProfileSyncService profileSync, RequestManager requestManager)
    {
        this.gameServer = gameServer;
        this.profileSync = profileSync;
        this.requestManager = requestManager;
    }

    public Task<ServerActionResult> RentBusiness(string lotId) =>
        Execute("RentBusiness", () => gameServer.TryRentBusinessAsync(lotId));

    public Task<ServerActionResult> AssignBusinessType(string lotId, string businessTypeId) =>
        Execute("AssignBusinessType", () => gameServer.TryAssignBusinessTypeAsync(lotId, businessTypeId));

    public Task<ServerActionResult> InstallModule(string lotId, string moduleId) =>
        Execute("InstallBusinessModule", () => gameServer.TryInstallBusinessModuleAsync(lotId, moduleId));

    public Task<ServerActionResult> AssignSupplier(string lotId, string supplierId) =>
        Execute("AssignSupplier", () => gameServer.TryAssignSupplierAsync(lotId, supplierId));

    public Task<ServerActionResult> HireWorker(string lotId, string roleId, string contactId) =>
        Execute("HireBusinessWorker", () => gameServer.TryHireBusinessWorkerAsync(lotId, roleId, contactId));

    public Task<ServerActionResult> OpenBusiness(string lotId) =>
        Execute("OpenBusiness", () => gameServer.TryOpenBusinessAsync(lotId));

    public Task<ServerActionResult> CloseBusiness(string lotId) =>
        Execute("CloseBusiness", () => gameServer.TryCloseBusinessAsync(lotId));

    public Task<ServerActionResult> SetMarkup(string lotId, int markupPercent) =>
        Execute("SetBusinessMarkup", () => gameServer.TrySetBusinessMarkupAsync(lotId, markupPercent));

    public Task<ServerActionResult> UnlockContact(string contactId) =>
        Execute("UnlockContact", () => gameServer.TryUnlockContactAsync(contactId));

    public Task<ServerActionResult> AddBusinessStock(string lotId, int amount) =>
        Execute("AddBusinessStock", () => gameServer.TryAddBusinessStockAsync(lotId, amount));

    public Task<ServerActionResult> AddBusinessShelfStock(string lotId, int amount) =>
        Execute("AddBusinessShelfStock", () => gameServer.TryAddBusinessShelfStockAsync(lotId, amount));

    public Task<ServerActionResult> ClearBusinessStock(string lotId) =>
        Execute("ClearBusinessStock", () => gameServer.TryClearBusinessStockAsync(lotId));

    public Task<ServerActionResult> ResetBusinesses() =>
        Execute("ResetBusinesses", () => gameServer.TryResetBusinessesAsync());

    private async Task<ServerActionResult> Execute(string label, Func<Task<ServerActionResult>> action)
    {
        if (gameServer == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ServerMissing", "IGameServer is not available.");
        }

        if (requestManager != null && !requestManager.TryStartRequest(label))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RequestBlocked", "Another request is in progress.");
        }

        try
        {
            BusinessDebugLog.Log($"[BusinessServer] {label} START");
            var result = await action();
            if (result?.ProfileSnapshot != null)
            {
                profileSync?.ApplySnapshot(result.ProfileSnapshot);
            }
            BusinessDebugLog.Log($"[BusinessServer] {label} Result: {(result != null && result.Success ? "Success" : "Fail")}");
            return result;
        }
        catch (Exception ex)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "Exception", ex.Message);
        }
        finally
        {
            requestManager?.FinishRequest();
        }
    }
}
