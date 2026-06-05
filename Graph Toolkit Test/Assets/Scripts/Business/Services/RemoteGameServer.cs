using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using GameGraph.Runtime.Quest;
using Prototype.Business.Data;
using Prototype.Business.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Prototype.Business.Services
{
	public class RemoteGameServer : IGameServer
	{
		private readonly string m_baseUrl;
		private readonly bool m_debugLog;
		private readonly string m_playerId;
		private readonly float m_timeoutSeconds;

		public RemoteGameServer(string baseUrl, string playerId, float timeoutSeconds, bool debugLog)
		{
			m_baseUrl = NormalizeBaseUrl(string.IsNullOrEmpty(baseUrl) ? "http://localhost:3000" : baseUrl);
			m_playerId = string.IsNullOrEmpty(playerId) ? "player" : playerId;
			if (timeoutSeconds <= 0f)
			{
				m_timeoutSeconds = 0f;
			}
			else
			{
				m_timeoutSeconds = Mathf.Clamp(timeoutSeconds, 0.1f, 120f);
			}

			m_debugLog = debugLog;
		}

		public Task<ServerActionResult> TryGetProfileAsync()
		{
			if (m_debugLog)
			{
				Debug.Log("[RemoteGameServer] action=get_profile");
			}

			var request = new RemoteProfileRequest
			{
				action = "get_profile",
				playerId = m_playerId
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryBuyBuildingAsync(
			string buildingId,
			QuestActionType questAction = QuestActionType.None,
			string questId = null)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=buy_building buildingId='{buildingId}' questAction='{questAction}' questId='{questId}'");
			}

			var request = new RemoteBuyBuildingRequest
			{
				action = "buy_building",
				playerId = m_playerId,
				data = new RemoteBuyBuildingData
				{
					buildingId = buildingId,
					questAction = MapQuestAction(questAction),
					questId = questId
				}
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryStartQuestAsync(string questId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=start_quest questId='{questId}'");
			}

			var request = new RemoteQuestRequest
			{
				action = "start_quest",
				playerId = m_playerId,
				data = new RemoteQuestData { questId = questId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryCompleteQuestAsync(string questId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=complete_quest questId='{questId}'");
			}

			var request = new RemoteQuestRequest
			{
				action = "complete_quest",
				playerId = m_playerId,
				data = new RemoteQuestData { questId = questId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryFailQuestAsync(string questId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=fail_quest questId='{questId}'");
			}

			var request = new RemoteQuestRequest
			{
				action = "fail_quest",
				playerId = m_playerId,
				data = new RemoteQuestData { questId = questId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryAddMoneyAsync(int amount)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=add_money amount={amount}");
			}

			var request = new RemoteMoneyRequest
			{
				action = "add_money",
				playerId = m_playerId,
				data = new RemoteMoneyData { amount = amount }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TrySpendMoneyAsync(int amount)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=spend_money amount={amount}");
			}

			var request = new RemoteMoneyRequest
			{
				action = "spend_money",
				playerId = m_playerId,
				data = new RemoteMoneyData { amount = amount }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryApplyPlayerDamageAsync(int amount)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=apply_player_damage amount={amount}");
			}

			var request = new RemotePlayerDamageRequest
			{
				action = "apply_player_damage",
				playerId = m_playerId,
				data = new RemotePlayerDamageData { amount = amount }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryStealAsync(int amount, bool canFail, int successChance)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=steal amount={amount} canFail={canFail} successChance={successChance}");
			}

			var request = new RemoteStealRequest
			{
				action = "steal",
				playerId = m_playerId,
				data = new RemoteStealData { amount = amount, canFail = canFail, successChance = successChance }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TrySaveCheckpointAsync(string graphId, string checkpointId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=save_checkpoint graphId='{graphId}' checkpointId='{checkpointId}'");
			}

			var request = new RemoteCheckpointRequest
			{
				action = "save_checkpoint",
				playerId = m_playerId,
				data = new RemoteCheckpointData
				{
					graphId = graphId,
					checkpointId = checkpointId
				}
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TrySubmitTradeOfferAsync(string buildingId, int offeredAmount)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=submit_trade_offer buildingId='{buildingId}' offeredAmount={offeredAmount}");
			}

			var request = new RemoteTradeOfferRequest
			{
				action = "submit_trade_offer",
				playerId = m_playerId,
				data = new RemoteTradeOfferData
				{
					buildingId = buildingId,
					offeredAmount = offeredAmount
				}
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryRentBusinessAsync(string lotId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=rent_business lotId='{lotId}'");
			}

			var request = new RemoteRentBusinessRequest
			{
				action = "rent_business",
				playerId = m_playerId,
				data = new RemoteRentBusinessData { lotId = lotId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryAssignBusinessTypeAsync(string lotId, string businessTypeId)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=assign_business_type lotId='{lotId}' businessTypeId='{businessTypeId}'");
			}

			var request = new RemoteAssignBusinessTypeRequest
			{
				action = "assign_business_type",
				playerId = m_playerId,
				data = new RemoteAssignBusinessTypeData { lotId = lotId, businessTypeId = businessTypeId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryInstallBusinessModuleAsync(string lotId, string moduleId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] install_business_module disabled lotId='{lotId}' moduleId='{moduleId}'");
			}

			return Task.FromResult(ServerActionResult.FailResult(
				ServerActionResult.ErrorType.GameLogicError,
				"BusinessModulesRemoved",
				"Business modules are no longer supported. Use business equipment items instead."));
		}

		public Task<ServerActionResult> TrySetBusinessEquipmentAsync(
			string lotId,
			string storageItemId,
			string cashDeskItemId,
			string shelfItemId)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=set_business_equipment lotId='{lotId}' storageItemId='{storageItemId}' cashDeskItemId='{cashDeskItemId}' shelfItemId='{shelfItemId}'");
			}

			var request = new RemoteSetBusinessEquipmentRequest
			{
				action = "set_business_equipment",
				playerId = m_playerId,
				data = new RemoteSetBusinessEquipmentData
				{
					lotId = lotId,
					storageItemId = storageItemId,
					cashDeskItemId = cashDeskItemId,
					shelfItemId = shelfItemId
				}
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryAssignSupplierAsync(string lotId, string supplierId)
		{
			string normalizedSupplierId = supplierId ?? string.Empty;
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=assign_supplier lotId='{lotId}' supplierId='{normalizedSupplierId}'");
			}

			var request = new RemoteAssignSupplierRequest
			{
				action = "assign_supplier",
				playerId = m_playerId,
				data = new RemoteAssignSupplierData { lotId = lotId, supplierId = normalizedSupplierId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryBuyItemAsync(string traderId, string itemId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=buy_item traderId='{traderId}' itemId='{itemId}'");
			}

			var request = new RemoteBuyItemRequest
			{
				action = "buy_item",
				playerId = m_playerId,
				data = new RemoteBuyItemData
				{
					traderId = traderId,
					itemId = itemId
				}
			};

			return SendRequestAsync(request);
		}

		public async Task<TraderItemsResponse> TryGetTraderItemsAsync(string traderId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=get_trader_items traderId='{traderId}'");
			}

			var request = new RemoteGetTraderItemsRequest
			{
				action = "get_trader_items",
				playerId = m_playerId,
				data = new RemoteGetTraderItemsData
				{
					traderId = traderId
				}
			};

			return await SendTraderItemsRequestAsync(request);
		}

		public Task<ServerActionResult> TryHireBusinessWorkerAsync(string lotId, string roleId, string contactId)
		{
			string normalizedContactId = contactId ?? string.Empty;
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=hire_business_worker lotId='{lotId}' roleId='{roleId}' contactId='{normalizedContactId}'");
			}

			var request = new RemoteHireBusinessWorkerRequest
			{
				action = "hire_business_worker",
				playerId = m_playerId,
				data = new RemoteHireBusinessWorkerData { lotId = lotId, roleId = roleId, contactId = normalizedContactId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryOpenBusinessAsync(string lotId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=open_business lotId='{lotId}'");
			}

			var request = new RemoteBusinessLotRequest
			{
				action = "open_business",
				playerId = m_playerId,
				data = new RemoteBusinessLotData { lotId = lotId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryCloseBusinessAsync(string lotId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=close_business lotId='{lotId}'");
			}

			var request = new RemoteBusinessLotRequest
			{
				action = "close_business",
				playerId = m_playerId,
				data = new RemoteBusinessLotData { lotId = lotId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TrySetBusinessMarkupAsync(string lotId, int markupPercent)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=set_business_markup lotId='{lotId}' markupPercent={markupPercent}");
			}

			var request = new RemoteSetBusinessMarkupRequest
			{
				action = "set_business_markup",
				playerId = m_playerId,
				data = new RemoteSetBusinessMarkupData { lotId = lotId, markupPercent = markupPercent }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TrySetBusinessAutoDeliveryAsync(string lotId, int dailyAmount)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=set_business_auto_delivery lotId='{lotId}' dailyAmount={dailyAmount}");
			}

			var request = new RemoteSetBusinessAutoDeliveryRequest
			{
				action = "set_business_auto_delivery",
				playerId = m_playerId,
				data = new RemoteSetBusinessAutoDeliveryData { lotId = lotId, dailyAmount = dailyAmount }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TrySimulateBusinessDayAsync(string lotId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=simulate_business_day lotId='{lotId}'");
			}

			var request = new RemoteSimulateBusinessDayRequest
			{
				action = "simulate_business_day",
				playerId = m_playerId,
				data = new RemoteSimulateBusinessDayData { lotId = lotId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryCollectBusinessProfitAsync(string lotId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=collect_business_profit lotId='{lotId}'");
			}

			var request = new RemoteBusinessLotRequest
			{
				action = "collect_business_profit",
				playerId = m_playerId,
				data = new RemoteBusinessLotData { lotId = lotId }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryConsumeNpcServiceAsync(string lotId, string serviceId, int requestedAmount)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=consume_npc_service lotId='{lotId}' serviceId='{serviceId}' requestedAmount={requestedAmount}");
			}

			var request = new RemoteConsumeNpcServiceRequest
			{
				action = "consume_npc_service",
				playerId = m_playerId,
				data = new RemoteConsumeNpcServiceData
				{
					lotId = lotId,
					serviceId = serviceId,
					requestedAmount = requestedAmount
				}
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryApplyBusinessDeliveryAsync(string lotId, int requestedAmount)
		{
			if (m_debugLog)
			{
				Debug.Log(
					$"[RemoteGameServer] action=apply_business_delivery lotId='{lotId}' requestedAmount={requestedAmount}");
			}

			var request = new RemoteApplyBusinessDeliveryRequest
			{
				action = "apply_business_delivery",
				playerId = m_playerId,
				data = new RemoteApplyBusinessDeliveryData
				{
					lotId = lotId,
					requestedAmount = requestedAmount
				}
			};

			return SendRequestAsync(request);
		}

		Task<ServerActionResult> IGameServer.TryCollectBusinessProfitAsync(string lotId)
		{
			return TryCollectBusinessProfitAsync(lotId);
		}

		public Task<ServerActionResult> TryUnlockContactAsync(string contactId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=unlock_contact contactId='{contactId}'");
			}

			var request = new RemoteUnlockContactRequest
			{
				action = "unlock_contact",
				playerId = m_playerId,
				data = new RemoteUnlockContactData { contactId = contactId }
			};

			return SendRequestAsync(request);
		}


		public Task<ServerActionResult> TryMoveBusinessStockToShelfAsync(string lotId, int amount)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=move_business_stock_to_shelf lotId='{lotId}' amount={amount}");
			}

			var request = new RemoteMoveBusinessStockToShelfRequest
			{
				action = "move_business_stock_to_shelf",
				playerId = m_playerId,
				data = new RemoteMoveBusinessStockToShelfData { lotId = lotId, amount = amount }
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryResetBusinessesAsync()
		{
			if (m_debugLog)
			{
				Debug.Log("[RemoteGameServer] action=reset_businesses");
			}

			var request = new RemoteResetBusinessesRequest
			{
				action = "reset_businesses",
				playerId = m_playerId
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryConstructSiteVisualAsync(string siteId, string visualId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=construct_site_visual siteId='{siteId}' visualId='{visualId}'");
			}

			var request = new RemoteConstructSiteVisualRequest
			{
				action = "construct_site_visual",
				playerId = m_playerId,
				data = new RemoteConstructSiteVisualData
				{
					siteId = siteId,
					visualId = visualId
				}
			};

			return SendRequestAsync(request);
		}

		public Task<ServerActionResult> TryRemoveSiteVisualAsync(string siteId)
		{
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] action=remove_site_visual siteId='{siteId}'");
			}

			var request = new RemoteSiteVisualRequest
			{
				action = "remove_site_visual",
				playerId = m_playerId,
				data = new RemoteSiteVisualData { siteId = siteId }
			};

			return SendRequestAsync(request);
		}

		private string NormalizeBaseUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return "http://127.0.0.1:3000";
			}

			string trimmed = url.TrimEnd('/');
			try
			{
				var uri = new Uri(trimmed);
				if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
				{
					var builder = new UriBuilder(uri)
					{
						Host = "127.0.0.1"
					};
					string normalized = builder.Uri.ToString().TrimEnd('/');
					if (m_debugLog)
					{
						Debug.Log($"[RemoteGameServer] Normalized baseUrl '{trimmed}' -> '{normalized}'");
					}

					return normalized;
				}
			}
			catch
			{
			}

			return trimmed;
		}

		private async Task<ServerActionResult> SendRequestAsync<T>(T requestPayload)
		{
			var url = $"{m_baseUrl}/api/action";
			string payload = JsonUtility.ToJson(requestPayload);

			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] POST {url}\n{payload}");
			}

			using var request = new UnityWebRequest(url, "POST");
			byte[] body = Encoding.UTF8.GetBytes(payload);
			request.uploadHandler = new UploadHandlerRaw(body);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			if (m_timeoutSeconds >= 1f)
			{
				request.timeout = Mathf.CeilToInt(m_timeoutSeconds);
			}

			var manualTimeout = false;
			var stopwatch = Stopwatch.StartNew();
			UnityWebRequestAsyncOperation op = request.SendWebRequest();
			while (!op.isDone)
			{
				if (m_timeoutSeconds > 0f && m_timeoutSeconds < 1f && stopwatch.Elapsed.TotalSeconds >= m_timeoutSeconds)
				{
					manualTimeout = true;
					request.Abort();
					break;
				}

				await Task.Yield();
			}

			double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

			if (manualTimeout)
			{
				if (m_debugLog)
				{
					string timeoutLabel = m_timeoutSeconds > 0f ? $"{m_timeoutSeconds:0.###}s" : "disabled";
					Debug.LogWarning(
						$"[RemoteGameServer] Network error: result=Timeout, error='Request timeout', url={url}, timeout={timeoutLabel}, elapsed={elapsedMs:0.0}ms");
				}

				return ServerActionResult.FailResult(ServerActionResult.ErrorType.Timeout, "Timeout", "Request timeout.");
			}

			string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;
			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] Response ({request.responseCode}) in {elapsedMs:0.0}ms: {responseText}");
				if (request.result != UnityWebRequest.Result.Success)
				{
					string timeoutLabel = m_timeoutSeconds > 0f ? $"{m_timeoutSeconds:0.###}s" : "disabled";
					Debug.LogWarning(
						$"[RemoteGameServer] Network error: result={request.result}, error='{request.error}', url={url}, timeout={timeoutLabel}, elapsed={elapsedMs:0.0}ms");
				}
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				ServerActionResult.ErrorType errorType = MapNetworkError(request);
				var errorCode = $"Http{request.responseCode}";
				string message = request.error;
				return ServerActionResult.FailResult(errorType, errorCode, message);
			}

			if (string.IsNullOrEmpty(responseText))
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "EmptyResponse",
					"Server returned empty response.");
			}

			RemoteActionResponse response = null;
			try
			{
				response = JsonUtility.FromJson<RemoteActionResponse>(responseText);
			}
			catch (Exception ex)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "InvalidJson", ex.Message);
			}

			if (response == null)
			{
				return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "InvalidResponse",
					"Response could not be parsed.");
			}

			ProfileSnapshot snapshot = response.profile != null ? MapProfileSnapshot(response.profile) : null;

			if (!response.success)
			{
				if (m_debugLog)
				{
					Debug.Log(
						$"[RemoteGameServer] Result: success=false errorCode={response.errorCode} message={response.message}");
				}

				return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError,
					response.errorCode ?? "ServerError", response.message);
			}

			if (m_debugLog)
			{
				Debug.Log($"[RemoteGameServer] Result: success=true message={response.message}");
			}

			return ServerActionResult.SuccessResult(snapshot, response.message);
		}

		private async Task<TraderItemsResponse> SendTraderItemsRequestAsync<T>(T requestPayload)
		{
			var url = $"{m_baseUrl}/api/action";
			string payload = JsonUtility.ToJson(requestPayload);

			using var request = new UnityWebRequest(url, "POST");
			byte[] body = Encoding.UTF8.GetBytes(payload);
			request.uploadHandler = new UploadHandlerRaw(body);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			if (m_timeoutSeconds >= 1f)
			{
				request.timeout = Mathf.CeilToInt(m_timeoutSeconds);
			}

			var manualTimeout = false;
			var stopwatch = Stopwatch.StartNew();
			UnityWebRequestAsyncOperation op = request.SendWebRequest();
			while (!op.isDone)
			{
				if (m_timeoutSeconds > 0f && m_timeoutSeconds < 1f && stopwatch.Elapsed.TotalSeconds >= m_timeoutSeconds)
				{
					manualTimeout = true;
					request.Abort();
					break;
				}

				await Task.Yield();
			}

			if (manualTimeout)
			{
				return new TraderItemsResponse
				{
					Success = false,
					ErrorCode = "Timeout",
					Message = "Request timeout."
				};
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				return new TraderItemsResponse
				{
					Success = false,
					ErrorCode = $"Http{request.responseCode}",
					Message = request.error
				};
			}

			string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;
			if (string.IsNullOrEmpty(responseText))
			{
				return new TraderItemsResponse
				{
					Success = false,
					ErrorCode = "EmptyResponse",
					Message = "Server returned empty response."
				};
			}

			RemoteTraderItemsResponse response;
			try
			{
				response = JsonUtility.FromJson<RemoteTraderItemsResponse>(responseText);
			}
			catch (Exception ex)
			{
				return new TraderItemsResponse
				{
					Success = false,
					ErrorCode = "InvalidJson",
					Message = ex.Message
				};
			}

			if (response == null)
			{
				return new TraderItemsResponse
				{
					Success = false,
					ErrorCode = "InvalidResponse",
					Message = "Response could not be parsed."
				};
			}

			var result = new TraderItemsResponse
			{
				Success = response.success,
				ErrorCode = response.errorCode,
				Message = response.message,
				TraderId = response.traderId,
				TraderName = response.traderName
			};

			if (response.items != null)
			{
				for (int i = 0; i < response.items.Length; i++)
				{
					RemoteTraderItemDto item = response.items[i];
					if (item == null || string.IsNullOrWhiteSpace(item.id))
					{
						continue;
					}

					result.Items.Add(new TraderItemDefinitionData
					{
						id = item.id,
						category = item.category,
						name = item.name,
						description = item.description,
						price = item.price,
						storageCapacity = item.storageCapacity,
						cashCapacity = item.cashCapacity,
						shelfCapacity = item.shelfCapacity
					});
				}
			}

			return result;
		}

		private ServerActionResult.ErrorType MapNetworkError(UnityWebRequest request)
		{
			if (request == null)
			{
				return ServerActionResult.ErrorType.NetworkError;
			}

			string error = request.error ?? string.Empty;
			if (error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return ServerActionResult.ErrorType.Timeout;
			}

			if (request.responseCode == 408 || request.responseCode == 504)
			{
				return ServerActionResult.ErrorType.Timeout;
			}

			return ServerActionResult.ErrorType.NetworkError;
		}

		private ProfileSnapshot MapProfileSnapshot(RemoteProfileDto profile)
		{
			var snapshot = new ProfileSnapshot
			{
				money = profile.money,
				bargaining = profile.bargaining,
				speech = profile.speech,
				trading = profile.trading,
				speed = profile.speed,
				damage = profile.damage,
				health = profile.health
			};

			if (profile.activeQuests != null)
			{
				snapshot.activeQuestIds.AddRange(profile.activeQuests);
			}

			if (profile.completedQuests != null)
			{
				snapshot.completedQuestIds.AddRange(profile.completedQuests);
			}

			if (profile.graphCheckpoints != null)
			{
				foreach (RemoteGraphCheckpointDto checkpoint in profile.graphCheckpoints)
				{
					if (checkpoint == null || string.IsNullOrEmpty(checkpoint.graphId))
					{
						continue;
					}

					snapshot.graphCheckpoints.Add(new GraphCheckpointSnapshot
					{
						graphId = checkpoint.graphId,
						checkpointId = checkpoint.checkpointId
					});
				}
			}

			if (profile.constructedSites != null)
			{
				foreach (RemoteConstructedSiteDto site in profile.constructedSites)
				{
					if (site == null || string.IsNullOrWhiteSpace(site.siteId))
					{
						continue;
					}

					snapshot.constructedSites.Add(new ConstructedSiteSnapshot
					{
						siteId = site.siteId,
						visualId = site.visualId,
						isConstructed = site.isConstructed
					});
				}
			}

			if (profile.businesses != null)
			{
				foreach (RemoteBusinessStateDto business in profile.businesses)
				{
					if (business == null || string.IsNullOrEmpty(business.instanceId))
					{
						continue;
					}

					var snapshotBusiness = new BusinessInstanceSnapshot
					{
						instanceId = business.instanceId,
						lotId = business.lotId,
						businessTypeId = business.businessTypeId,
						isOpen = business.isOpen,
						storageStock = business.storageStock,
						shelfStock = business.shelfStock,
						storageItemId = business.storageItemId,
						cashDeskItemId = business.cashDeskItemId,
						shelfItemId = business.shelfItemId,
						services = business.services != null ? new List<string>(business.services) : new List<string>(),
						autoDeliveryPerDay = business.autoDeliveryPerDay,
						markupPercent = business.markupPercent,
						dayRevenue = business.dayRevenue,
						dayExpenses = business.dayExpenses,
						profit = business.profit,
						hiredCashierContactId = business.hiredCashierContactId,
						hiredMerchContactId = business.hiredMerchContactId,
						hiredLogistContactId = business.hiredLogistContactId
					};

					snapshot.businesses.Add(snapshotBusiness);
				}
			}

			if (profile.knownContacts != null)
			{
				snapshot.knownContacts.AddRange(profile.knownContacts);
			}

			if (profile.items != null)
			{
				snapshot.items.AddRange(profile.items);
			}

			return snapshot;
		}

		private static string MapQuestAction(QuestActionType action)
		{
			return action switch
			{
				QuestActionType.StartQuest => "start",
				QuestActionType.CompleteQuest => "complete",
				_ => null
			};
		}

		[Serializable]
		private class RemoteProfileRequest
		{
			public string action;
			public string playerId;
		}

		[Serializable]
		private class RemoteBuyBuildingRequest
		{
			public string action;
			public string playerId;
			public RemoteBuyBuildingData data;
		}

		[Serializable]
		private class RemoteBuyBuildingData
		{
			public string buildingId;
			public string questAction;
			public string questId;
		}

		[Serializable]
		private class RemoteQuestRequest
		{
			public string action;
			public string playerId;
			public RemoteQuestData data;
		}

		[Serializable]
		private class RemoteQuestData
		{
			public string questId;
		}

		[Serializable]
		private class RemoteMoneyRequest
		{
			public string action;
			public string playerId;
			public RemoteMoneyData data;
		}

		[Serializable]
		private class RemoteMoneyData
		{
			public int amount;
		}

		[Serializable]
		private class RemotePlayerDamageRequest
		{
			public string action;
			public string playerId;
			public RemotePlayerDamageData data;
		}

		[Serializable]
		private class RemotePlayerDamageData
		{
			public int amount;
		}

		[Serializable]
		private class RemoteStealRequest
		{
			public string action;
			public string playerId;
			public RemoteStealData data;
		}

		[Serializable]
		private class RemoteStealData
		{
			public int amount;
			public bool canFail;
			public int successChance;
		}

		[Serializable]
		private class RemoteCheckpointRequest
		{
			public string action;
			public string playerId;
			public RemoteCheckpointData data;
		}

		[Serializable]
		private class RemoteCheckpointData
		{
			public string graphId;
			public string checkpointId;
		}

		[Serializable]
		private class RemoteTradeOfferRequest
		{
			public string action;
			public string playerId;
			public RemoteTradeOfferData data;
		}

		[Serializable]
		private class RemoteTradeOfferData
		{
			public string buildingId;
			public int offeredAmount;
		}

		[Serializable]
		private class RemoteRentBusinessRequest
		{
			public string action;
			public string playerId;
			public RemoteRentBusinessData data;
		}

		[Serializable]
		private class RemoteRentBusinessData
		{
			public string lotId;
		}

		[Serializable]
		private class RemoteAssignBusinessTypeRequest
		{
			public string action;
			public string playerId;
			public RemoteAssignBusinessTypeData data;
		}

		[Serializable]
		private class RemoteAssignBusinessTypeData
		{
			public string lotId;
			public string businessTypeId;
		}

		[Serializable]
		private class RemoteInstallBusinessModuleRequest
		{
			public string action;
			public string playerId;
			public RemoteInstallBusinessModuleData data;
		}

		[Serializable]
		private class RemoteInstallBusinessModuleData
		{
			public string lotId;
			public string moduleId;
		}

		[Serializable]
		private class RemoteAssignSupplierRequest
		{
			public string action;
			public string playerId;
			public RemoteAssignSupplierData data;
		}

		[Serializable]
		private class RemoteAssignSupplierData
		{
			public string lotId;
			public string supplierId;
		}

		[Serializable]
		private class RemoteSetBusinessEquipmentRequest
		{
			public string action;
			public string playerId;
			public RemoteSetBusinessEquipmentData data;
		}

		[Serializable]
		private class RemoteSetBusinessEquipmentData
		{
			public string lotId;
			public string storageItemId;
			public string cashDeskItemId;
			public string shelfItemId;
		}

		[Serializable]
		private class RemoteHireBusinessWorkerRequest
		{
			public string action;
			public string playerId;
			public RemoteHireBusinessWorkerData data;
		}

		[Serializable]
		private class RemoteHireBusinessWorkerData
		{
			public string lotId;
			public string roleId;
			public string contactId;
		}

		[Serializable]
		private class RemoteBuyItemRequest
		{
			public string action;
			public string playerId;
			public RemoteBuyItemData data;
		}

		[Serializable]
		private class RemoteBuyItemData
		{
			public string traderId;
			public string itemId;
		}

		[Serializable]
		private class RemoteGetTraderItemsRequest
		{
			public string action;
			public string playerId;
			public RemoteGetTraderItemsData data;
		}

		[Serializable]
		private class RemoteGetTraderItemsData
		{
			public string traderId;
		}

		[Serializable]
		private class RemoteBusinessLotRequest
		{
			public string action;
			public string playerId;
			public RemoteBusinessLotData data;
		}

		[Serializable]
		private class RemoteBusinessLotData
		{
			public string lotId;
		}

		[Serializable]
		private class RemoteSetBusinessMarkupRequest
		{
			public string action;
			public string playerId;
			public RemoteSetBusinessMarkupData data;
		}

		[Serializable]
		private class RemoteSetBusinessMarkupData
		{
			public string lotId;
			public int markupPercent;
		}

		[Serializable]
		private class RemoteSetBusinessAutoDeliveryRequest
		{
			public string action;
			public string playerId;
			public RemoteSetBusinessAutoDeliveryData data;
		}

		[Serializable]
		private class RemoteSetBusinessAutoDeliveryData
		{
			public string lotId;
			public int dailyAmount;
		}

		[Serializable]
		private class RemoteSimulateBusinessDayRequest
		{
			public string action;
			public string playerId;
			public RemoteSimulateBusinessDayData data;
		}

		[Serializable]
		private class RemoteSimulateBusinessDayData
		{
			public string lotId;
		}

		[Serializable]
		private class RemoteMoveBusinessStockToShelfRequest
		{
			public string action;
			public string playerId;
			public RemoteMoveBusinessStockToShelfData data;
		}

		[Serializable]
		private class RemoteMoveBusinessStockToShelfData
		{
			public string lotId;
			public int amount;
		}

		[Serializable]
		private class RemoteConsumeNpcServiceRequest
		{
			public string action;
			public string playerId;
			public RemoteConsumeNpcServiceData data;
		}

		[Serializable]
		private class RemoteConsumeNpcServiceData
		{
			public string lotId;
			public string serviceId;
			public int requestedAmount;
		}

		[Serializable]
		private class RemoteApplyBusinessDeliveryRequest
		{
			public string action;
			public string playerId;
			public RemoteApplyBusinessDeliveryData data;
		}

		[Serializable]
		private class RemoteApplyBusinessDeliveryData
		{
			public string lotId;
			public int requestedAmount;
		}

		[Serializable]
		private class RemoteResetBusinessesRequest
		{
			public string action;
			public string playerId;
		}

		[Serializable]
		private class RemoteConstructSiteVisualRequest
		{
			public string action;
			public string playerId;
			public RemoteConstructSiteVisualData data;
		}

		[Serializable]
		private class RemoteConstructSiteVisualData
		{
			public string siteId;
			public string visualId;
		}

		[Serializable]
		private class RemoteSiteVisualRequest
		{
			public string action;
			public string playerId;
			public RemoteSiteVisualData data;
		}

		[Serializable]
		private class RemoteSiteVisualData
		{
			public string siteId;
		}

		[Serializable]
		private class RemoteUnlockContactRequest
		{
			public string action;
			public string playerId;
			public RemoteUnlockContactData data;
		}

		[Serializable]
		private class RemoteUnlockContactData
		{
			public string contactId;
		}

		[Serializable]
		private class RemoteActionResponse
		{
			public bool success;
			public string errorCode;
			public string message;
			public RemoteProfileDto profile;
		}

		[Serializable]
		private class RemoteProfileDto
		{
			public int money;
			public string[] activeQuests;
			public string[] completedQuests;
			public int bargaining;
			public int speech;
			public int trading;
			public int speed;
			public int damage;
			public int health;
			public RemoteGraphCheckpointDto[] graphCheckpoints;
			public RemoteConstructedSiteDto[] constructedSites;
			public RemoteBusinessStateDto[] businesses;
			public string[] knownContacts;
			public string[] items;
		}

		[Serializable]
		private class RemoteGraphCheckpointDto
		{
			public string graphId;
			public string checkpointId;
		}

		[Serializable]
		private class RemoteConstructedSiteDto
		{
			public string siteId;
			public string visualId;
			public bool isConstructed;
		}

		[Serializable]
		private class RemoteBusinessStateDto
		{
			public string instanceId;
			public string lotId;
			public string businessTypeId;
			public bool isOpen;
			public int storageStock;
			public int shelfStock;
			public string storageItemId;
			public string cashDeskItemId;
			public string shelfItemId;
			public string[] services;
			public int autoDeliveryPerDay;
			public int markupPercent;
			public int dayRevenue;
			public int dayExpenses;
			public int profit;
			public string hiredCashierContactId;
			public string hiredMerchContactId;
			public string hiredLogistContactId;
		}

		[Serializable]
		private class RemoteTraderItemsResponse
		{
			public bool success;
			public string errorCode;
			public string message;
			public string traderId;
			public string traderName;
			public RemoteTraderItemDto[] items;
		}

		[Serializable]
		private class RemoteTraderItemDto
		{
			public string id;
			public string category;
			public string name;
			public string description;
			public int price;
			public int storageCapacity;
			public int cashCapacity;
			public int shelfCapacity;
		}
	}
}
