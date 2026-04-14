using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LocalGameServer : IGameServer
{
    private readonly GameRuntimeState runtime;
    private readonly GameDataRepository dataRepository;
    private readonly BusinessDefinitionsRepository businessRepository;
    private readonly int minDelayMs;
    private readonly int maxDelayMs;
    private readonly float networkErrorChance;
    private readonly float timeoutChance;
    private readonly Dictionary<string, string> graphCheckpoints = new Dictionary<string, string>();
    private readonly List<ConstructedSiteSnapshot> constructedSites = new List<ConstructedSiteSnapshot>();
    private readonly List<BusinessInstanceSnapshot> businesses = new List<BusinessInstanceSnapshot>();
    private readonly HashSet<string> knownContacts = new HashSet<string>();
    private static readonly System.Random Random = new System.Random();

    public LocalGameServer(
        GameRuntimeState runtime,
        GameDataRepository dataRepository,
        BusinessDefinitionsRepository businessRepository,
        int minDelayMs = 100,
        int maxDelayMs = 500,
        float networkErrorChance = 0.05f,
        float timeoutChance = 0.03f)
    {
        this.runtime = runtime;
        this.dataRepository = dataRepository;
        this.businessRepository = businessRepository;
        this.minDelayMs = Mathf.Clamp(minDelayMs, 0, 60000);
        this.maxDelayMs = Mathf.Max(this.minDelayMs, Mathf.Clamp(maxDelayMs, 0, 60000));
        this.networkErrorChance = Mathf.Clamp01(networkErrorChance);
        this.timeoutChance = Mathf.Clamp01(timeoutChance);
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

    public async Task<ServerActionResult> TryBuyBuildingAsync(string buildingId, QuestActionType questAction = QuestActionType.None, string questId = null)
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

        QuestDefinitionData questDefinition = null;
        QuestState questState = null;
        if (questAction != QuestActionType.None)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestIdEmpty", "Quest id is empty.");
            }

            questDefinition = dataRepository != null ? dataRepository.GetQuestById(questId) : null;
            if (questDefinition == null)
            {
                return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound", "Quest definition not found.");
            }

            questState = GetQuestById(questId);

            if (questAction == QuestActionType.StartQuest)
            {
                if (questState != null && questState.Status == QuestStatus.Active)
                {
                    return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestAlreadyActive", "Quest already active.");
                }
                if (questState != null && questState.Status == QuestStatus.Completed)
                {
                    return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestAlreadyCompleted", "Quest already completed.");
                }
            }
            else if (questAction == QuestActionType.CompleteQuest)
            {
                if (questState == null || questState.Status != QuestStatus.Active)
                {
                    return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest is not active.");
                }
            }
        }

        int cost = building.Definition.purchaseCost;
        if (runtime.Player.Money < cost)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney", "Not enough money.");
        }

        if (!TryBuyBuilding(building, runtime.Player))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuyFailed", "Buy building failed.");
        }

        TryConstructSiteFromBuildingDefinition(building.Definition);

        if (questAction == QuestActionType.StartQuest)
        {
            AcceptQuest(questDefinition);
        }
        else if (questAction == QuestActionType.CompleteQuest)
        {
            if (questDefinition != null && runtime != null && runtime.Player != null)
            {
                runtime.Player.Money += questDefinition.rewardMoney;
            }
            CompleteQuest(questId);
        }

        return ServerActionResult.SuccessResult(BuildSnapshot(), "Buy building success.");
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

        QuestState quest = GetQuestById(questId);
        if (quest == null || quest.Status != QuestStatus.Active)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest is not active.");
        }

        CompleteQuest(questId);
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

        QuestState quest = GetQuestById(questId);
        if (quest == null || quest.Status != QuestStatus.Active)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive", "Quest is not active.");
        }

        FailQuest(questId);
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

    public async Task<ServerActionResult> TrySaveCheckpointAsync(string graphId, string checkpointId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrEmpty(graphId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "GraphIdEmpty", "Graph id is empty.");
        }

        if (string.IsNullOrEmpty(checkpointId))
        {
            graphCheckpoints.Remove(graphId);
            return ServerActionResult.SuccessResult(BuildSnapshot(), "Checkpoint cleared.");
        }

        graphCheckpoints[graphId] = checkpointId;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Checkpoint saved.");
    }

    public async Task<ServerActionResult> TrySubmitTradeOfferAsync(string buildingId, int offeredAmount)
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

        if (offeredAmount < 1)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidOffer", "Offer must be >= 1.");
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

        int fullPrice = building.Definition.purchaseCost;
        if (offeredAmount > fullPrice)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "OfferTooHigh", "Offer exceeds full price.");
        }

        if (runtime.Player.Money < offeredAmount)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney", "Not enough money.");
        }

        int baseChance = GetBaseTradeChance(offeredAmount, fullPrice);
        int tradingBonus = runtime.Player.Trading;
        int finalChance = Mathf.Clamp(baseChance + tradingBonus, 0, 95);

        int roll;
        lock (Random)
        {
            roll = Random.Next(0, 100);
        }

        if (roll >= finalChance)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "TradeRejected", "Trade offer rejected.");
        }

        runtime.Player.Money -= offeredAmount;
        building.IsOwned = true;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Trade offer accepted.");
    }

    public async Task<ServerActionResult> TryRentBusinessAsync(string lotId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (FindBusinessByLotId(lotId) != null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessAlreadyRented", "Business already rented for this lot.");
        }

        var lotDef = dataRepository?.GetLotById(lotId);
        if (lotDef == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotNotFound", "Lot not found.");
        }

        var business = new BusinessInstanceSnapshot
        {
            instanceId = $"local_{System.Guid.NewGuid():N}",
            lotId = lotId,
            businessTypeId = null,
            isRented = true,
            isOpen = false,
            rentPerDay = lotDef.rentPerDay < 0 ? 0 : lotDef.rentPerDay,
            installedModules = new List<string>(),
            storageCapacity = 0,
            shelfCapacity = 0,
            storageStock = 0,
            shelfStock = 0,
            selectedSupplierId = null,
            autoDeliveryPerDay = 0,
            markupPercent = 0,
            hiredCashierContactId = null,
            hiredMerchContactId = null
        };

        businesses.Add(business);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Rent business success.");
    }

    public async Task<ServerActionResult> TryConstructSiteVisualAsync(string siteId, string visualId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(siteId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SiteIdEmpty", "siteId is required.");
        }

        if (string.IsNullOrWhiteSpace(visualId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "VisualIdEmpty", "visualId is required.");
        }

        var site = FindConstructedSiteBySiteId(siteId);
        if (site == null)
        {
            site = new ConstructedSiteSnapshot
            {
                siteId = siteId.Trim()
            };
            constructedSites.Add(site);
        }

        site.isConstructed = true;
        site.visualId = visualId.Trim();
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Construct site visual success.");
    }

    public async Task<ServerActionResult> TryRemoveSiteVisualAsync(string siteId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(siteId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SiteIdEmpty", "siteId is required.");
        }

        var site = FindConstructedSiteBySiteId(siteId);
        if (site != null)
        {
            constructedSites.Remove(site);
        }

        return ServerActionResult.SuccessResult(BuildSnapshot(), "Remove site visual success.");
    }

    public async Task<ServerActionResult> TryAssignBusinessTypeAsync(string lotId, string businessTypeId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (string.IsNullOrWhiteSpace(businessTypeId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeIdEmpty", "businessTypeId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        var lotDef = dataRepository?.GetLotById(lotId);
        if (lotDef == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotNotFound", "Lot not found.");
        }

        if (lotDef.allowedBusinessTypes != null && lotDef.allowedBusinessTypes.Count > 0 &&
            !lotDef.allowedBusinessTypes.Contains(businessTypeId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeNotAllowedForLot", "Business type not allowed for this lot.");
        }

        var typeDef = businessRepository?.GetBusinessType(businessTypeId);
        if (typeDef == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeNotFound", "Business type not found.");
        }

        business.businessTypeId = businessTypeId;
        business.storageCapacity = typeDef.defaultStorageCapacity;
        business.shelfCapacity = typeDef.defaultShelfCapacity;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Assign business type success.");
    }

    public async Task<ServerActionResult> TryInstallBusinessModuleAsync(string lotId, string moduleId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ModuleIdEmpty", "moduleId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        var moduleDef = businessRepository?.GetModule(moduleId);
        if (moduleDef == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ModuleNotFound", "Module not found.");
        }

        if (business.installedModules.Contains(moduleId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ModuleAlreadyInstalled", "Module already installed.");
        }

        if (runtime == null || runtime.Player == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing", "Runtime state is not available.");
        }

        if (runtime.Player.Money < moduleDef.installCost)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney", "Not enough money.");
        }

        runtime.Player.Money -= moduleDef.installCost;
        business.installedModules.Add(moduleId);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Install module success.");
    }

    public async Task<ServerActionResult> TryAssignSupplierAsync(string lotId, string supplierId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (string.IsNullOrWhiteSpace(supplierId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SupplierIdEmpty", "supplierId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        var supplier = businessRepository?.GetSupplier(supplierId);
        if (supplier == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SupplierNotFound", "Supplier not found.");
        }

        if (!knownContacts.Contains(supplierId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactNotKnown", "Supplier contact not unlocked.");
        }

        business.selectedSupplierId = supplierId;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Assign supplier success.");
    }

    public async Task<ServerActionResult> TryHireBusinessWorkerAsync(string lotId, string roleId, string contactId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RoleIdEmpty", "roleId is required.");
        }

        if (string.IsNullOrWhiteSpace(contactId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactIdEmpty", "contactId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        var role = businessRepository?.GetStaffRole(roleId);
        if (role == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidWorkerRole", "Worker role not found.");
        }

        if (!knownContacts.Contains(contactId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactNotKnown", "Contact not unlocked.");
        }

        if (roleId == "cashier")
        {
            business.hiredCashierContactId = contactId;
        }
        else if (roleId == "merchandiser")
        {
            business.hiredMerchContactId = contactId;
        }
        else
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidWorkerRole", "Unsupported worker role.");
        }

        return ServerActionResult.SuccessResult(BuildSnapshot(), "Hire worker success.");
    }

    public async Task<ServerActionResult> TryOpenBusinessAsync(string lotId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        if (!business.isRented)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotRented", "Business is not rented.");
        }

        if (string.IsNullOrWhiteSpace(business.businessTypeId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeMissing", "Business type not assigned.");
        }

        var typeDef = businessRepository?.GetBusinessType(business.businessTypeId);
        if (typeDef == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeNotFound", "Business type not found.");
        }

        var required = typeDef.requiredModules ?? new List<string>();
        foreach (var moduleId in required)
        {
            if (!business.installedModules.Contains(moduleId))
            {
                return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "MissingRequiredModules", "Missing required modules.");
            }
        }

        business.isOpen = true;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Open business success.");
    }

    public async Task<ServerActionResult> TryCloseBusinessAsync(string lotId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        business.isOpen = false;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Close business success.");
    }

    public async Task<ServerActionResult> TrySetBusinessMarkupAsync(string lotId, int markupPercent)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (markupPercent < 0 || markupPercent > 100)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidMarkup", "markupPercent must be between 0 and 100.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        business.markupPercent = markupPercent;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Set markup success.");
    }

    public async Task<ServerActionResult> TryUnlockContactAsync(string contactId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(contactId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactIdEmpty", "contactId is required.");
        }

        knownContacts.Add(contactId);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Unlock contact success.");
    }

    public async Task<ServerActionResult> TryAddBusinessStockAsync(string lotId, int amount)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (amount <= 0)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "AmountInvalid", "amount must be positive.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        if (business.installedModules == null || !business.installedModules.Contains("storage"))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "StorageNotInstalled", "Storage module not installed.");
        }

        int capacity = business.storageCapacity;
        int space = capacity > 0 ? capacity - business.storageStock : 0;
        if (space <= 0)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "StorageFull", "Storage is full.");
        }

        int added = amount > space ? space : amount;
        business.storageStock += added;
        return ServerActionResult.SuccessResult(BuildSnapshot(), $"Added stock: {added}.");
    }

    public async Task<ServerActionResult> TryAddBusinessShelfStockAsync(string lotId, int amount)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        if (amount <= 0)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "AmountInvalid", "amount must be positive.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        if (business.installedModules == null || !business.installedModules.Contains("shelves"))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ShelvesNotInstalled", "Shelves module not installed.");
        }

        int capacity = business.shelfCapacity;
        int space = capacity > 0 ? capacity - business.shelfStock : 0;
        if (space <= 0)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ShelvesFull", "Shelves are full.");
        }

        int added = amount > space ? space : amount;
        business.shelfStock += added;
        return ServerActionResult.SuccessResult(BuildSnapshot(), $"Added shelf stock: {added}.");
    }

    public async Task<ServerActionResult> TryClearBusinessStockAsync(string lotId)
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        if (string.IsNullOrWhiteSpace(lotId))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty", "lotId is required.");
        }

        var business = FindBusinessByLotId(lotId);
        if (business == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound", "Business not found.");
        }

        business.storageStock = 0;
        business.shelfStock = 0;
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Cleared business stock.");
    }

    public async Task<ServerActionResult> TryResetBusinessesAsync()
    {
        int delayMs = NextDelayMs();
        var networkIssue = SampleNetworkIssue();
        Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
        await Task.Delay(delayMs);

        if (networkIssue != ServerActionResult.ErrorType.None)
        {
            return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
        }

        businesses.Clear();
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Businesses reset.");
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

    private BusinessInstanceSnapshot FindBusinessByLotId(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId)) return null;

        for (int i = 0; i < businesses.Count; i++)
        {
            var business = businesses[i];
            if (business != null && business.lotId == lotId)
            {
                return business;
            }
        }

        return null;
    }

    private ConstructedSiteSnapshot FindConstructedSiteBySiteId(string siteId)
    {
        if (string.IsNullOrWhiteSpace(siteId)) return null;

        string normalizedSiteId = siteId.Trim();
        for (int i = 0; i < constructedSites.Count; i++)
        {
            var site = constructedSites[i];
            if (site != null && site.siteId == normalizedSiteId)
            {
                return site;
            }
        }

        return null;
    }

    private void TryConstructSiteFromBuildingDefinition(BuildingDefinitionData definition)
    {
        if (definition == null || string.IsNullOrWhiteSpace(definition.siteId) || string.IsNullOrWhiteSpace(definition.visualId))
        {
            return;
        }

        string normalizedSiteId = definition.siteId.Trim();
        string normalizedVisualId = definition.visualId.Trim();

        var existing = FindConstructedSiteBySiteId(normalizedSiteId);
        if (existing == null)
        {
            constructedSites.Add(new ConstructedSiteSnapshot
            {
                siteId = normalizedSiteId,
                visualId = normalizedVisualId,
                isConstructed = true
            });
            return;
        }

        existing.visualId = normalizedVisualId;
        existing.isConstructed = true;
    }

    private ProfileSnapshot BuildSnapshot()
    {
        var snapshot = new ProfileSnapshot();

        if (runtime != null && runtime.Player != null)
        {
            snapshot.Money = runtime.Player.Money;
            snapshot.Bargaining = runtime.Player.Bargaining;
            snapshot.Speech = runtime.Player.Speech;
            snapshot.Trading = runtime.Player.Trading;
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

                if (building.IsOwned)
                {
                    var buildingSnapshot = new BuildingStateSnapshot
                    {
                        id = building.Definition.id,
                        owned = true,
                        level = building.Level,
                        currentIncome = building.CurrentIncome,
                        currentExpenses = building.CurrentExpenses
                    };
                    snapshot.BuildingStates.Add(buildingSnapshot);
                    snapshot.OwnedBuildingIds.Add(building.Definition.id);
                }
            }
        }

        if (graphCheckpoints.Count > 0)
        {
            foreach (var pair in graphCheckpoints)
            {
                snapshot.GraphCheckpoints.Add(new GraphCheckpointSnapshot
                {
                    graphId = pair.Key,
                    checkpointId = pair.Value
                });
            }
        }

        if (constructedSites.Count > 0)
        {
            foreach (var site in constructedSites)
            {
                if (site == null || string.IsNullOrWhiteSpace(site.siteId))
                {
                    continue;
                }

                snapshot.ConstructedSites.Add(new ConstructedSiteSnapshot
                {
                    siteId = site.siteId,
                    visualId = site.isConstructed ? site.visualId : null,
                    isConstructed = site.isConstructed && !string.IsNullOrWhiteSpace(site.visualId)
                });
            }
        }

        if (businesses.Count > 0)
        {
            foreach (var business in businesses)
            {
                if (business == null || string.IsNullOrEmpty(business.instanceId))
                {
                    continue;
                }

                var businessSnapshot = new BusinessInstanceSnapshot
                {
                    instanceId = business.instanceId,
                    lotId = business.lotId,
                    businessTypeId = business.businessTypeId,
                    isRented = business.isRented,
                    isOpen = business.isOpen,
                    rentPerDay = business.rentPerDay,
                    storageCapacity = business.storageCapacity,
                    shelfCapacity = business.shelfCapacity,
                    storageStock = business.storageStock,
                    shelfStock = business.shelfStock,
                    selectedSupplierId = business.selectedSupplierId,
                    autoDeliveryPerDay = business.autoDeliveryPerDay,
                    markupPercent = business.markupPercent,
                    hiredCashierContactId = business.hiredCashierContactId,
                    hiredMerchContactId = business.hiredMerchContactId
                };

                if (business.installedModules != null)
                {
                    businessSnapshot.installedModules.AddRange(business.installedModules);
                }

                snapshot.Businesses.Add(businessSnapshot);
            }
        }

        if (knownContacts.Count > 0)
        {
            snapshot.KnownContacts.AddRange(knownContacts);
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

        QuestState existing = GetQuestById(questDefinition.id);
        if (existing != null && existing.Status == QuestStatus.Active)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestAlreadyActive", "Quest already active.");
        }

        AcceptQuest(questDefinition);
        return ServerActionResult.SuccessResult(BuildSnapshot(), "Start quest success.");
    }

    private bool TryBuyBuilding(BuildingState building, PlayerProfileState player)
    {
        if (building == null || player == null || building.Definition == null)
        {
            return false;
        }

        if (building.IsOwned)
        {
            return false;
        }

        int cost = building.Definition.purchaseCost;
        if (player.Money < cost)
        {
            return false;
        }

        player.Money -= cost;
        building.IsOwned = true;
        return true;
    }

    private void AcceptQuest(QuestDefinitionData definition)
    {
        if (runtime == null || runtime.Quests == null || definition == null)
        {
            return;
        }

        if (HasActiveQuest(definition.id))
        {
            return;
        }

        QuestState quest = GetQuestById(definition.id);
        if (quest == null)
        {
            quest = new QuestState(definition);
            runtime.Quests.Add(quest);
        }
        else
        {
            quest.Definition = definition;
        }

        quest.Status = QuestStatus.Active;
    }

    private void CompleteQuest(string questId)
    {
        QuestState quest = GetQuestById(questId);
        if (quest != null && quest.Status == QuestStatus.Active)
        {
            quest.Status = QuestStatus.Completed;
        }
    }

    private void FailQuest(string questId)
    {
        QuestState quest = GetQuestById(questId);
        if (quest != null && quest.Status == QuestStatus.Active)
        {
            quest.Status = QuestStatus.Failed;
        }
    }

    private bool HasActiveQuest(string questId)
    {
        QuestState quest = GetQuestById(questId);
        return quest != null && quest.Status == QuestStatus.Active;
    }

    private QuestState GetQuestById(string questId)
    {
        if (runtime == null || runtime.Quests == null || string.IsNullOrEmpty(questId))
        {
            return null;
        }

        foreach (QuestState quest in runtime.Quests)
        {
            if (quest?.Definition != null && quest.Definition.id == questId)
            {
                return quest;
            }
        }

        return null;
    }

    private int NextDelayMs()
    {
        lock (Random)
        {
            return Random.Next(minDelayMs, maxDelayMs + 1);
        }
    }

    private int GetBaseTradeChance(int offeredAmount, int fullPrice)
    {
        if (fullPrice <= 0)
        {
            return 0;
        }

        float percent = (offeredAmount / (float)fullPrice) * 100f;
        if (percent < 10f)
        {
            return 10;
        }

        if (percent < 50f)
        {
            return 40;
        }

        return 50;
    }

    private ServerActionResult.ErrorType SampleNetworkIssue()
    {
        double roll;
        lock (Random)
        {
            roll = Random.NextDouble();
        }

        if (roll < networkErrorChance)
        {
            return ServerActionResult.ErrorType.NetworkError;
        }

        if (roll < networkErrorChance + timeoutChance)
        {
            return ServerActionResult.ErrorType.Timeout;
        }

        return ServerActionResult.ErrorType.None;
    }
}
