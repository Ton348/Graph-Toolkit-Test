using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameGraph.Runtime.Quest;
using Prototype.Business.Data;
using Prototype.Business.Runtime;
using Sample.Runtime.GameData;
using Sample.Runtime.Runtime;
using UnityEngine;
using Random = System.Random;

namespace Prototype.Business.Services
{
	public class LocalGameServer : IGameServer
	{
		private static readonly Random s_random = new();
		private readonly List<BusinessInstanceSnapshot> m_businesses = new();
		private readonly BusinessDefinitionsRepository m_businessRepository;
		private readonly List<ConstructedSiteSnapshot> m_constructedSites = new();
		private readonly GameDataRepository m_dataRepository;
		private readonly Dictionary<string, string> m_graphCheckpoints = new();
		private readonly HashSet<string> m_knownContacts = new();
		private readonly int m_maxDelayMs;
		private readonly int m_minDelayMs;
		private readonly float m_networkErrorChance;
		private readonly GameRuntimeState m_runtime;
		private readonly float m_timeoutChance;

		public LocalGameServer(
			GameRuntimeState runtime,
			GameDataRepository dataRepository,
			BusinessDefinitionsRepository businessRepository,
			int minDelayMs = 100,
			int maxDelayMs = 500,
			float networkErrorChance = 0.05f,
			float timeoutChance = 0.03f)
		{
			m_runtime = runtime;
			m_dataRepository = dataRepository;
			m_businessRepository = businessRepository;
			m_minDelayMs = Mathf.Clamp(minDelayMs, 0, 60000);
			m_maxDelayMs = Mathf.Max(m_minDelayMs, Mathf.Clamp(maxDelayMs, 0, 60000));
			m_networkErrorChance = Mathf.Clamp01(networkErrorChance);
			m_timeoutChance = Mathf.Clamp01(timeoutChance);
		}

		public async Task<ServerActionResult> TryGetProfileAsync()
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (m_runtime == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			return ServerActionResult.SuccessResult(BuildSnapshot(), "Profile fetch success.");
		}

		public async Task<ServerActionResult> TryBuyBuildingAsync(
			string buildingId,
			QuestActionType questAction = QuestActionType.None,
			string questId = null)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrEmpty(buildingId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingIdEmpty",
					"Building id is empty.");
			}

			if (m_runtime == null || m_runtime.buildings == null || m_runtime.player == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			BuildingState building = FindBuilding(buildingId, m_runtime.buildings);
			if (building == null || building.definition == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingNotFound",
					"Building not found.");
			}

			if (building.isOwned)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingAlreadyOwned",
					"Building already owned.");
			}

			QuestDefinitionData questDefinition = null;
			QuestState questState = null;
			if (questAction != QuestActionType.None)
			{
				if (string.IsNullOrWhiteSpace(questId))
				{
					return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestIdEmpty",
						"Quest id is empty.");
				}

				questDefinition = m_dataRepository != null ? m_dataRepository.GetQuestById(questId) : null;
				if (questDefinition == null)
				{
					return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound",
						"Quest definition not found.");
				}

				questState = GetQuestById(questId);

				if (questAction == QuestActionType.StartQuest)
				{
					if (questState != null && questState.status == QuestStatus.Active)
					{
						return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError,
							"QuestAlreadyActive", "Quest already active.");
					}

					if (questState != null && questState.status == QuestStatus.Completed)
					{
						return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError,
							"QuestAlreadyCompleted", "Quest already completed.");
					}
				}
				else if (questAction == QuestActionType.CompleteQuest)
				{
					if (questState == null || questState.status != QuestStatus.Active)
					{
						return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive",
							"Quest is not active.");
					}
				}
			}

			int cost = building.definition.purchaseCost;
			if (m_runtime.player.money < cost)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney",
					"Not enough money.");
			}

			if (!TryBuyBuilding(building, m_runtime.player))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuyFailed",
					"Buy building failed.");
			}

			TryConstructSiteFromBuildingDefinition(building.definition);

			if (questAction == QuestActionType.StartQuest)
			{
				AcceptQuest(questDefinition);
			}
			else if (questAction == QuestActionType.CompleteQuest)
			{
				if (questDefinition != null && m_runtime != null && m_runtime.player != null)
				{
					m_runtime.player.money += questDefinition.rewardMoney;
				}

				CompleteQuest(questId);
			}

			return ServerActionResult.SuccessResult(BuildSnapshot(), "Buy building success.");
		}

		public async Task<ServerActionResult> TryStartQuestAsync(string questId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrEmpty(questId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound",
					"Quest id is empty.");
			}

			QuestDefinitionData questDefinition = m_dataRepository != null ? m_dataRepository.GetQuestById(questId) : null;
			if (questDefinition == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound",
					"Quest definition not found.");
			}

			return TryStartQuestInternal(questDefinition);
		}

		public async Task<ServerActionResult> TryCompleteQuestAsync(string questId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrEmpty(questId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive",
					"Quest id is empty.");
			}

			if (m_runtime == null || m_runtime.quests == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			QuestState quest = GetQuestById(questId);
			if (quest == null || quest.status != QuestStatus.Active)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive",
					"Quest is not active.");
			}

			CompleteQuest(questId);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Complete quest success.");
		}

		public async Task<ServerActionResult> TryFailQuestAsync(string questId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrEmpty(questId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive",
					"Quest id is empty.");
			}

			if (m_runtime == null || m_runtime.quests == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			QuestState quest = GetQuestById(questId);
			if (quest == null || quest.status != QuestStatus.Active)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotActive",
					"Quest is not active.");
			}

			FailQuest(questId);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Fail quest success.");
		}

		public async Task<ServerActionResult> TryAddMoneyAsync(int amount)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (m_runtime == null || m_runtime.player == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			if (amount <= 0)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidAmount",
					"Amount must be > 0.");
			}

			m_runtime.player.money += amount;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Add money success.");
		}

		public async Task<ServerActionResult> TrySpendMoneyAsync(int amount)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (m_runtime == null || m_runtime.player == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			if (amount <= 0)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidAmount",
					"Amount must be > 0.");
			}

			if (m_runtime.player.money < amount)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney",
					"Not enough money.");
			}

			m_runtime.player.money -= amount;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Spend money success.");
		}

		public async Task<ServerActionResult> TryStealAsync(int amount, bool canFail, int successChance)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (m_runtime == null || m_runtime.player == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			if (!canFail)
			{
				m_runtime.player.money += amount;
				return ServerActionResult.SuccessResult(BuildSnapshot(), "Steal success.");
			}

			int roll;
			lock (s_random)
			{
				roll = s_random.Next(0, 100);
			}

			bool success = roll < Mathf.Clamp(successChance, 0, 100);
			if (!success)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "StealFailed",
					"Steal failed.");
			}

			m_runtime.player.money += amount;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Steal success.");
		}

		public async Task<ServerActionResult> TrySaveCheckpointAsync(string graphId, string checkpointId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrEmpty(graphId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "GraphIdEmpty",
					"Graph id is empty.");
			}

			if (string.IsNullOrEmpty(checkpointId))
			{
				m_graphCheckpoints.Remove(graphId);
				return ServerActionResult.SuccessResult(BuildSnapshot(), "Checkpoint cleared.");
			}

			m_graphCheckpoints[graphId] = checkpointId;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Checkpoint saved.");
		}

		public async Task<ServerActionResult> TrySubmitTradeOfferAsync(string buildingId, int offeredAmount)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrEmpty(buildingId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingIdEmpty",
					"Building id is empty.");
			}

			if (offeredAmount < 1)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidOffer",
					"Offer must be >= 1.");
			}

			if (m_runtime == null || m_runtime.buildings == null || m_runtime.player == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			BuildingState building = FindBuilding(buildingId, m_runtime.buildings);
			if (building == null || building.definition == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingNotFound",
					"Building not found.");
			}

			if (building.isOwned)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BuildingAlreadyOwned",
					"Building already owned.");
			}

			int fullPrice = building.definition.purchaseCost;
			if (offeredAmount > fullPrice)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "OfferTooHigh",
					"Offer exceeds full price.");
			}

			if (m_runtime.player.money < offeredAmount)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney",
					"Not enough money.");
			}

			int baseChance = GetBaseTradeChance(offeredAmount, fullPrice);
			int tradingBonus = m_runtime.player.trading;
			int finalChance = Mathf.Clamp(baseChance + tradingBonus, 0, 95);

			int roll;
			lock (s_random)
			{
				roll = s_random.Next(0, 100);
			}

			if (roll >= finalChance)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "TradeRejected",
					"Trade offer rejected.");
			}

			m_runtime.player.money -= offeredAmount;
			building.isOwned = true;
			TryConstructSiteFromBuildingDefinition(building.definition);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Trade offer accepted.");
		}

		public async Task<ServerActionResult> TryRentBusinessAsync(string lotId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (FindBusinessByLotId(lotId) != null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessAlreadyRented",
					"Business already rented for this lot.");
			}

			LotDefinitionData lotDef = m_dataRepository?.GetLotById(lotId);
			if (lotDef == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotNotFound",
					"Lot not found.");
			}

			var business = new BusinessInstanceSnapshot
			{
				instanceId = $"local_{Guid.NewGuid():N}",
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

			m_businesses.Add(business);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Rent business success.");
		}

		public async Task<ServerActionResult> TryConstructSiteVisualAsync(string siteId, string visualId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(siteId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SiteIdEmpty",
					"siteId is required.");
			}

			if (string.IsNullOrWhiteSpace(visualId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "VisualIdEmpty",
					"visualId is required.");
			}

			ConstructedSiteSnapshot site = FindConstructedSiteBySiteId(siteId);
			if (site == null)
			{
				site = new ConstructedSiteSnapshot
				{
					siteId = siteId.Trim()
				};
				m_constructedSites.Add(site);
			}

			site.isConstructed = true;
			site.visualId = visualId.Trim();
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Construct site visual success.");
		}

		public async Task<ServerActionResult> TryRemoveSiteVisualAsync(string siteId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(siteId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SiteIdEmpty",
					"siteId is required.");
			}

			ConstructedSiteSnapshot site = FindConstructedSiteBySiteId(siteId);
			if (site != null)
			{
				m_constructedSites.Remove(site);
			}

			return ServerActionResult.SuccessResult(BuildSnapshot(), "Remove site visual success.");
		}

		public async Task<ServerActionResult> TryAssignBusinessTypeAsync(string lotId, string businessTypeId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeIdEmpty",
					"businessTypeId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			LotDefinitionData lotDef = m_dataRepository?.GetLotById(lotId);
			if (lotDef == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotNotFound",
					"Lot not found.");
			}

			if (lotDef.allowedBusinessTypes != null && lotDef.allowedBusinessTypes.Count > 0 &&
			    !lotDef.allowedBusinessTypes.Contains(businessTypeId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError,
					"BusinessTypeNotAllowedForLot", "Business type not allowed for this lot.");
			}

			BusinessTypeDefinitionData typeDef = m_businessRepository?.GetBusinessType(businessTypeId);
			if (typeDef == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeNotFound",
					"Business type not found.");
			}

			business.businessTypeId = businessTypeId;
			business.storageCapacity = typeDef.defaultStorageCapacity;
			business.shelfCapacity = typeDef.defaultShelfCapacity;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Assign business type success.");
		}

		public async Task<ServerActionResult> TryInstallBusinessModuleAsync(string lotId, string moduleId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (string.IsNullOrWhiteSpace(moduleId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ModuleIdEmpty",
					"moduleId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			BusinessModuleDefinitionData moduleDef = m_businessRepository?.GetModule(moduleId);
			if (moduleDef == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ModuleNotFound",
					"Module not found.");
			}

			if (business.installedModules.Contains(moduleId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ModuleAlreadyInstalled",
					"Module already installed.");
			}

			if (m_runtime == null || m_runtime.player == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			if (m_runtime.player.money < moduleDef.installCost)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "NotEnoughMoney",
					"Not enough money.");
			}

			m_runtime.player.money -= moduleDef.installCost;
			business.installedModules.Add(moduleId);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Install module success.");
		}

		public async Task<ServerActionResult> TryAssignSupplierAsync(string lotId, string supplierId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (string.IsNullOrWhiteSpace(supplierId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SupplierIdEmpty",
					"supplierId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			SupplierDefinitionData supplier = m_businessRepository?.GetSupplier(supplierId);
			if (supplier == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "SupplierNotFound",
					"Supplier not found.");
			}

			if (!m_knownContacts.Contains(supplierId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactNotKnown",
					"Supplier contact not unlocked.");
			}

			business.selectedSupplierId = supplierId;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Assign supplier success.");
		}

		public async Task<ServerActionResult> TryHireBusinessWorkerAsync(string lotId, string roleId, string contactId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (string.IsNullOrWhiteSpace(roleId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RoleIdEmpty",
					"roleId is required.");
			}

			if (string.IsNullOrWhiteSpace(contactId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactIdEmpty",
					"contactId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			StaffRoleDefinitionData role = m_businessRepository?.GetStaffRole(roleId);
			if (role == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidWorkerRole",
					"Worker role not found.");
			}

			if (!m_knownContacts.Contains(contactId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactNotKnown",
					"Contact not unlocked.");
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
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidWorkerRole",
					"Unsupported worker role.");
			}

			return ServerActionResult.SuccessResult(BuildSnapshot(), "Hire worker success.");
		}

		public async Task<ServerActionResult> TryOpenBusinessAsync(string lotId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			if (!business.isRented)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotRented",
					"Business is not rented.");
			}

			if (string.IsNullOrWhiteSpace(business.businessTypeId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeMissing",
					"Business type not assigned.");
			}

			BusinessTypeDefinitionData typeDef = m_businessRepository?.GetBusinessType(business.businessTypeId);
			if (typeDef == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessTypeNotFound",
					"Business type not found.");
			}

			List<string> required = typeDef.requiredModules ?? new List<string>();
			foreach (string moduleId in required)
			{
				if (!business.installedModules.Contains(moduleId))
				{
					return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError,
						"MissingRequiredModules", "Missing required modules.");
				}
			}

			business.isOpen = true;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Open business success.");
		}

		public async Task<ServerActionResult> TryCloseBusinessAsync(string lotId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			business.isOpen = false;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Close business success.");
		}

		public async Task<ServerActionResult> TrySetBusinessMarkupAsync(string lotId, int markupPercent)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (markupPercent < 0 || markupPercent > 100)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "InvalidMarkup",
					"markupPercent must be between 0 and 100.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			business.markupPercent = markupPercent;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Set markup success.");
		}

		public async Task<ServerActionResult> TryUnlockContactAsync(string contactId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(contactId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ContactIdEmpty",
					"contactId is required.");
			}

			m_knownContacts.Add(contactId);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Unlock contact success.");
		}

		public async Task<ServerActionResult> TryAddBusinessStockAsync(string lotId, int amount)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (amount <= 0)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "AmountInvalid",
					"amount must be positive.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			if (business.installedModules == null || !business.installedModules.Contains("storage"))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "StorageNotInstalled",
					"Storage module not installed.");
			}

			int capacity = business.storageCapacity;
			int space = capacity > 0 ? capacity - business.storageStock : 0;
			if (space <= 0)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "StorageFull",
					"Storage is full.");
			}

			int added = amount > space ? space : amount;
			business.storageStock += added;
			return ServerActionResult.SuccessResult(BuildSnapshot(), $"Added stock: {added}.");
		}

		public async Task<ServerActionResult> TryAddBusinessShelfStockAsync(string lotId, int amount)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			if (amount <= 0)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "AmountInvalid",
					"amount must be positive.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			if (business.installedModules == null || !business.installedModules.Contains("shelves"))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ShelvesNotInstalled",
					"Shelves module not installed.");
			}

			int capacity = business.shelfCapacity;
			int space = capacity > 0 ? capacity - business.shelfStock : 0;
			if (space <= 0)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "ShelvesFull",
					"Shelves are full.");
			}

			int added = amount > space ? space : amount;
			business.shelfStock += added;
			return ServerActionResult.SuccessResult(BuildSnapshot(), $"Added shelf stock: {added}.");
		}

		public async Task<ServerActionResult> TryClearBusinessStockAsync(string lotId)
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			if (string.IsNullOrWhiteSpace(lotId))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "LotIdEmpty",
					"lotId is required.");
			}

			BusinessInstanceSnapshot business = FindBusinessByLotId(lotId);
			if (business == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "BusinessNotFound",
					"Business not found.");
			}

			business.storageStock = 0;
			business.shelfStock = 0;
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Cleared business stock.");
		}

		public async Task<ServerActionResult> TryResetBusinessesAsync()
		{
			int delayMs = NextDelayMs();
			ServerActionResult.ErrorType networkIssue = SampleNetworkIssue();
			Debug.Log($"[LocalGameServer] Delay: {delayMs}ms");
			await Task.Delay(delayMs);

			if (networkIssue != ServerActionResult.ErrorType.None)
			{
				return ServerActionResult.FailResult(networkIssue, networkIssue.ToString(), "Network error.");
			}

			m_businesses.Clear();
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Businesses reset.");
		}

		private static BuildingState FindBuilding(string buildingId, List<BuildingState> buildings)
		{
			if (buildings == null)
			{
				return null;
			}

			for (var i = 0; i < buildings.Count; i++)
			{
				BuildingState state = buildings[i];
				if (state != null && state.definition != null && state.definition.id == buildingId)
				{
					return state;
				}
			}

			return null;
		}

		private BusinessInstanceSnapshot FindBusinessByLotId(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return null;
			}

			for (var i = 0; i < m_businesses.Count; i++)
			{
				BusinessInstanceSnapshot business = m_businesses[i];
				if (business != null && business.lotId == lotId)
				{
					return business;
				}
			}

			return null;
		}

		private ConstructedSiteSnapshot FindConstructedSiteBySiteId(string siteId)
		{
			if (string.IsNullOrWhiteSpace(siteId))
			{
				return null;
			}

			string normalizedSiteId = siteId.Trim();
			for (var i = 0; i < m_constructedSites.Count; i++)
			{
				ConstructedSiteSnapshot site = m_constructedSites[i];
				if (site != null && site.siteId == normalizedSiteId)
				{
					return site;
				}
			}

			return null;
		}

		private void TryConstructSiteFromBuildingDefinition(BuildingDefinitionData definition)
		{
			if (definition == null || string.IsNullOrWhiteSpace(definition.siteId) ||
			    string.IsNullOrWhiteSpace(definition.visualId))
			{
				return;
			}

			string normalizedSiteId = definition.siteId.Trim();
			string normalizedVisualId = definition.visualId.Trim();

			ConstructedSiteSnapshot existing = FindConstructedSiteBySiteId(normalizedSiteId);
			if (existing == null)
			{
				m_constructedSites.Add(new ConstructedSiteSnapshot
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

			if (m_runtime != null && m_runtime.player != null)
			{
				snapshot.money = m_runtime.player.money;
				snapshot.bargaining = m_runtime.player.bargaining;
				snapshot.speech = m_runtime.player.speech;
				snapshot.trading = m_runtime.player.trading;
				snapshot.speed = m_runtime.player.speed;
				snapshot.damage = m_runtime.player.damage;
				snapshot.health = m_runtime.player.health;
			}

			if (m_runtime != null && m_runtime.quests != null)
			{
				foreach (QuestState quest in m_runtime.quests)
				{
					if (quest == null || quest.definition == null)
					{
						continue;
					}

					string id = quest.definition.id;
					if (quest.status == QuestStatus.Active)
					{
						snapshot.activeQuestIds.Add(id);
					}
					else if (quest.status == QuestStatus.Completed)
					{
						snapshot.completedQuestIds.Add(id);
					}
				}
			}

			if (m_runtime != null && m_runtime.buildings != null)
			{
				foreach (BuildingState building in m_runtime.buildings)
				{
					if (building == null || building.definition == null)
					{
						continue;
					}

					if (building.isOwned)
					{
						var buildingSnapshot = new BuildingStateSnapshot
						{
							id = building.definition.id,
							owned = true,
							level = building.level,
							currentIncome = building.currentIncome,
							currentExpenses = building.currentExpenses
						};
						snapshot.buildingStates.Add(buildingSnapshot);
						snapshot.ownedBuildingIds.Add(building.definition.id);
					}
				}
			}

			if (m_graphCheckpoints.Count > 0)
			{
				foreach (KeyValuePair<string, string> pair in m_graphCheckpoints)
				{
					snapshot.graphCheckpoints.Add(new GraphCheckpointSnapshot
					{
						graphId = pair.Key,
						checkpointId = pair.Value
					});
				}
			}

			if (m_constructedSites.Count > 0)
			{
				foreach (ConstructedSiteSnapshot site in m_constructedSites)
				{
					if (site == null || string.IsNullOrWhiteSpace(site.siteId))
					{
						continue;
					}

					snapshot.constructedSites.Add(new ConstructedSiteSnapshot
					{
						siteId = site.siteId,
						visualId = site.isConstructed ? site.visualId : null,
						isConstructed = site.isConstructed && !string.IsNullOrWhiteSpace(site.visualId)
					});
				}
			}

			if (m_businesses.Count > 0)
			{
				foreach (BusinessInstanceSnapshot business in m_businesses)
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

					snapshot.businesses.Add(businessSnapshot);
				}
			}

			if (m_knownContacts.Count > 0)
			{
				snapshot.knownContacts.AddRange(m_knownContacts);
			}

			return snapshot;
		}

		private ServerActionResult TryStartQuestInternal(QuestDefinitionData questDefinition)
		{
			if (questDefinition == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestNotFound",
					"Quest definition is null.");
			}

			if (m_runtime == null || m_runtime.quests == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "RuntimeMissing",
					"Runtime state is not available.");
			}

			QuestState existing = GetQuestById(questDefinition.id);
			if (existing != null && existing.status == QuestStatus.Active)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, "QuestAlreadyActive",
					"Quest already active.");
			}

			AcceptQuest(questDefinition);
			return ServerActionResult.SuccessResult(BuildSnapshot(), "Start quest success.");
		}

		private bool TryBuyBuilding(BuildingState building, PlayerProfileState player)
		{
			if (building == null || player == null || building.definition == null)
			{
				return false;
			}

			if (building.isOwned)
			{
				return false;
			}

			int cost = building.definition.purchaseCost;
			if (player.money < cost)
			{
				return false;
			}

			player.money -= cost;
			building.isOwned = true;
			return true;
		}

		private void AcceptQuest(QuestDefinitionData definition)
		{
			if (m_runtime == null || m_runtime.quests == null || definition == null)
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
				m_runtime.quests.Add(quest);
			}
			else
			{
				quest.definition = definition;
			}

			quest.status = QuestStatus.Active;
		}

		private void CompleteQuest(string questId)
		{
			QuestState quest = GetQuestById(questId);
			if (quest != null && quest.status == QuestStatus.Active)
			{
				quest.status = QuestStatus.Completed;
			}
		}

		private void FailQuest(string questId)
		{
			QuestState quest = GetQuestById(questId);
			if (quest != null && quest.status == QuestStatus.Active)
			{
				quest.status = QuestStatus.Failed;
			}
		}

		private bool HasActiveQuest(string questId)
		{
			QuestState quest = GetQuestById(questId);
			return quest != null && quest.status == QuestStatus.Active;
		}

		private QuestState GetQuestById(string questId)
		{
			if (m_runtime == null || m_runtime.quests == null || string.IsNullOrEmpty(questId))
			{
				return null;
			}

			foreach (QuestState quest in m_runtime.quests)
			{
				if (quest?.definition != null && quest.definition.id == questId)
				{
					return quest;
				}
			}

			return null;
		}

		private int NextDelayMs()
		{
			lock (s_random)
			{
				return s_random.Next(m_minDelayMs, m_maxDelayMs + 1);
			}
		}

		private int GetBaseTradeChance(int offeredAmount, int fullPrice)
		{
			if (fullPrice <= 0)
			{
				return 0;
			}

			float percent = offeredAmount / (float)fullPrice * 100f;
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
			lock (s_random)
			{
				roll = s_random.NextDouble();
			}

			if (roll < m_networkErrorChance)
			{
				return ServerActionResult.ErrorType.NetworkError;
			}

			if (roll < m_networkErrorChance + m_timeoutChance)
			{
				return ServerActionResult.ErrorType.Timeout;
			}

			return ServerActionResult.ErrorType.None;
		}
	}
}