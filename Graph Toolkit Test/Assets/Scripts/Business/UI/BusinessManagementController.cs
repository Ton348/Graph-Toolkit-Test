using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prototype.Business.Data;
using Prototype.Business.Runtime;
using Prototype.Business.Services;
using Sample.Runtime.GameData;

namespace Prototype.Business.UI
{
	public sealed class BusinessManagementController
	{
		private BusinessDetailsView m_view;
		private BusinessActionFacade m_actionFacade;
		private BusinessRuntimeService m_runtimeService;
		private BusinessDefinitionsRepository m_definitions;
		private GameDataRepository m_gameData;
		private Action<string> m_setStatus;
		private Action<string> m_onBusinessChanged;
		private bool m_isApplying;

		public void Initialize(
			BusinessDetailsView view,
			BusinessActionFacade actionFacade,
			BusinessRuntimeService runtimeService,
			BusinessDefinitionsRepository definitions,
			GameDataRepository gameData,
			Action<string> setStatus,
			Action<string> onBusinessChanged)
		{
			Dispose();

			m_view = view;
			m_actionFacade = actionFacade;
			m_runtimeService = runtimeService;
			m_definitions = definitions;
			m_gameData = gameData;
			m_setStatus = setStatus;
			m_onBusinessChanged = onBusinessChanged;

			if (m_view != null)
			{
				m_view.openCloseClicked += OnOpenCloseClicked;
				m_view.businessChanged += OnBusinessChanged;
			}
		}

		public void Dispose()
		{
			if (m_view != null)
			{
				m_view.openCloseClicked -= OnOpenCloseClicked;
				m_view.businessChanged -= OnBusinessChanged;
			}

			m_view = null;
			m_actionFacade = null;
			m_runtimeService = null;
			m_definitions = null;
			m_gameData = null;
			m_setStatus = null;
			m_onBusinessChanged = null;
			m_isApplying = false;
		}

		private void OnBusinessChanged(string lotId)
		{
			m_onBusinessChanged?.Invoke(lotId);
		}

		private async void OnOpenCloseClicked()
		{
			if (m_isApplying)
			{
				return;
			}

			if (m_view == null || m_actionFacade == null || m_runtimeService == null)
			{
				return;
			}

			string lotId = m_view.GetSelectedBusinessId();
			if (string.IsNullOrWhiteSpace(lotId))
			{
				SetStatus("Fail: LotIdEmpty");
				return;
			}

			m_isApplying = true;
			try
			{
				BusinessInstanceSnapshot business = m_runtimeService.GetBusinessView(lotId);
				bool isOpen = business != null && business.isOpen;

				if (isOpen)
				{
					await CloseBusinessAsync(lotId);
				}
				else
				{
					await OpenBusinessAsync(lotId);
				}
			}
			finally
			{
				m_isApplying = false;
			}
		}

		private async Task OpenBusinessAsync(string lotId)
		{
			if (!await ApplyBusinessConfigurationAsync(lotId))
			{
				return;
			}

			if (!await RunActionCheckedAsync(m_actionFacade.OpenBusiness(lotId), "Открытие бизнеса"))
			{
				return;
			}

			m_view?.SetBusinessOpenState(true);
			m_onBusinessChanged?.Invoke(lotId);
		}

		private async Task CloseBusinessAsync(string lotId)
		{
			if (!await RunActionCheckedAsync(m_actionFacade.CloseBusiness(lotId), "Закрытие бизнеса"))
			{
				return;
			}

			m_view?.SetBusinessOpenState(false);
			m_onBusinessChanged?.Invoke(lotId);
		}

		private async Task<bool> ApplyBusinessConfigurationAsync(string lotId)
		{
			BusinessInstanceSnapshot business = m_runtimeService.GetBusinessView(lotId);
			string currentTypeId = business != null ? business.businessTypeId : null;
			string businessTypeId = ResolveBusinessTypeId(lotId, currentTypeId);
			if (!string.IsNullOrWhiteSpace(businessTypeId) && businessTypeId != currentTypeId)
			{
				if (!await RunActionCheckedAsync(m_actionFacade.AssignBusinessType(lotId, businessTypeId), "Назначение типа бизнеса"))
				{
					return false;
				}
			}

			business = m_runtimeService.GetBusinessView(lotId);
			HashSet<string> installed = business != null && business.installedModules != null
				? new HashSet<string>(business.installedModules)
				: new HashSet<string>();

			var modules = new[]
			{
				new { Id = NormalizeId(m_view.GetPendingStorageId()), Step = "Установка склада" },
				new { Id = NormalizeId(m_view.GetPendingCashDeskId()), Step = "Установка касс" },
				new { Id = NormalizeId(m_view.GetPendingShelfId()), Step = "Установка полок" }
			};

			foreach (var module in modules)
			{
				if (string.IsNullOrWhiteSpace(module.Id) || installed.Contains(module.Id))
				{
					continue;
				}

				if (!await RunActionCheckedAsync(m_actionFacade.InstallModule(lotId, module.Id), module.Step))
				{
					return false;
				}

				installed.Add(module.Id);
			}

			business = m_runtimeService.GetBusinessView(lotId);
			string supplierId = NormalizeId(m_view.GetPendingSupplierId());
			if (string.IsNullOrWhiteSpace(supplierId))
			{
				if (business != null && !string.IsNullOrWhiteSpace(business.selectedSupplierId))
				{
					if (!await RunActionCheckedAsync(m_actionFacade.ClearSupplier(lotId), "Снятие поставщика"))
					{
						return false;
					}
				}
			}
			else if (business == null || business.selectedSupplierId != supplierId)
			{
				if (!await RunActionCheckedAsync(m_actionFacade.AssignSupplier(lotId, supplierId), "Назначение поставщика"))
				{
					return false;
				}
			}

			business = m_runtimeService.GetBusinessView(lotId);
			string cashierId = NormalizeId(m_view.GetPendingCashierId());
			if (string.IsNullOrWhiteSpace(cashierId))
			{
				if (business != null && !string.IsNullOrWhiteSpace(business.hiredCashierContactId))
				{
					if (!await RunActionCheckedAsync(m_actionFacade.ClearWorker(lotId, "cashier"), "Снятие кассира"))
					{
						return false;
					}
				}
			}
			else if (business == null || business.hiredCashierContactId != cashierId)
			{
				if (!await RunActionCheckedAsync(m_actionFacade.HireWorker(lotId, "cashier", cashierId), "Назначение кассира"))
				{
					return false;
				}
			}

			business = m_runtimeService.GetBusinessView(lotId);
			string merchandiserId = NormalizeId(m_view.GetPendingMerchandiserId());
			if (string.IsNullOrWhiteSpace(merchandiserId))
			{
				if (business != null && !string.IsNullOrWhiteSpace(business.hiredMerchContactId))
				{
					if (!await RunActionCheckedAsync(m_actionFacade.ClearWorker(lotId, "merchandiser"), "Снятие мерчендайзера"))
					{
						return false;
					}
				}
			}
			else if (business == null || business.hiredMerchContactId != merchandiserId)
			{
				if (!await RunActionCheckedAsync(m_actionFacade.HireWorker(lotId, "merchandiser", merchandiserId), "Назначение мерчендайзера"))
				{
					return false;
				}
			}

			business = m_runtimeService.GetBusinessView(lotId);
			int pendingPrice = m_view.GetPendingPrice();
			if (business == null || business.markupPercent != pendingPrice)
			{
				if (!await RunActionCheckedAsync(m_actionFacade.SetMarkup(lotId, pendingPrice), "Установка цены"))
				{
					return false;
				}
			}

			return true;
		}

		private string ResolveBusinessTypeId(string lotId, string currentTypeId)
		{
			if (!string.IsNullOrWhiteSpace(currentTypeId))
			{
				return currentTypeId;
			}

			if (m_gameData == null || m_definitions == null || string.IsNullOrWhiteSpace(lotId))
			{
				return null;
			}

			LotDefinitionData lot = m_gameData.GetLotById(lotId);
			if (lot == null || lot.allowedBusinessTypes == null || lot.allowedBusinessTypes.Count == 0)
			{
				return null;
			}

			foreach (string typeId in lot.allowedBusinessTypes)
			{
				if (string.IsNullOrWhiteSpace(typeId))
				{
					continue;
				}

				BusinessTypeDefinitionData type = m_definitions.GetBusinessType(typeId);
				if (type != null)
				{
					return type.id;
				}
			}

			return null;
		}

		private async Task<bool> RunActionCheckedAsync(Task<ServerActionResult> actionTask, string stepName)
		{
			if (actionTask == null)
			{
				SetStatus($"{stepName}: action is null.");
				return false;
			}

			ServerActionResult result = await actionTask;
			if (result == null)
			{
				SetStatus($"{stepName}: no response.");
				return false;
			}

			if (!result.Success)
			{
				SetStatus($"{stepName}: {result.ErrorCode}");
				return false;
			}

			SetStatus(string.IsNullOrWhiteSpace(result.Message)
				? $"{stepName}: ok"
				: $"{stepName}: {result.Message}");
			return true;
		}

		private Task<bool> RunActionCheckedAsync(Task<ServerActionResult> actionTask)
		{
			return RunActionCheckedAsync(actionTask, "Выполнение действия");
		}

		private void SetStatus(string message)
		{
			m_setStatus?.Invoke(message);
		}

		private static string NormalizeId(string value)
		{
			return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
		}
	}
}
