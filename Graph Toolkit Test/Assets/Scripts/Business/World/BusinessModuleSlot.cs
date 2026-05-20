using Prototype.Business.Runtime;
using Prototype.Business.Services;
using UnityEngine;

namespace Prototype.Business.World
{
	public class BusinessModuleSlot : MonoBehaviour
	{
		public string moduleId;
		public BusinessWorldRuntime worldRuntime;
		public Transform moduleVisual;
		public KeyCode interactKey = KeyCode.E;
		public string playerTag = "Player";

		private bool m_isInstalled;
		private BusinessStateSyncService m_stateSync;

		private void Start()
		{
			RefreshFromState();
		}

		private void OnEnable()
		{
			ResolveStateSync();
			RefreshFromState();
			if (m_stateSync != null)
			{
				m_stateSync.stateChanged += RefreshFromState;
			}
		}

		private void OnDisable()
		{
			if (m_stateSync != null)
			{
				m_stateSync.stateChanged -= RefreshFromState;
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (!other.CompareTag(playerTag))
			{
				return;
			}

			if (!Input.GetKeyDown(interactKey))
			{
				return;
			}

			PlayerCarryItem carrier =
				other.GetComponentInParent<PlayerCarryItem>() ?? other.GetComponent<PlayerCarryItem>();
			if (carrier == null)
			{
				return;
			}

			TryInstallAsync(carrier);
		}

		private void ResolveStateSync()
		{
			if (worldRuntime == null)
			{
				worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
			}

			m_stateSync = worldRuntime != null && worldRuntime.bootstrap != null
				? worldRuntime.bootstrap.BusinessStateSyncService
				: null;
		}

		private async void TryInstallAsync(PlayerCarryItem carrier)
		{
			await System.Threading.Tasks.Task.Yield();

			if (m_isInstalled)
			{
				BusinessDebugLog.Log(
					$"[BusinessWorld] Module already installed '{moduleId}' lotId='{worldRuntime?.lotId}'.");
				return;
			}

			if (worldRuntime == null)
			{
				worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
			}

			if (worldRuntime == null || string.IsNullOrWhiteSpace(moduleId))
			{
				BusinessDebugLog.Warn("[BusinessWorld] Module slot missing runtime or moduleId.");
				return;
			}

			BusinessDebugLog.Log($"[BusinessWorld] Module slot '{moduleId}' is legacy and disabled for lotId='{worldRuntime.lotId}'.");
		}

		public void RefreshFromState()
		{
			if (worldRuntime == null)
			{
				worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
			}

			BusinessInstanceSnapshot business = worldRuntime != null ? worldRuntime.GetBusiness() : null;
			if (business == null)
			{
				return;
			}

			m_isInstalled = business != null && IsInstalledByModule(business);
			SetVisual(m_isInstalled);
		}

		private bool IsInstalledByModule(BusinessInstanceSnapshot business)
		{
			string key = moduleId == null ? string.Empty : moduleId.Trim().ToLowerInvariant();
			if (key.Contains("storage"))
			{
				return !string.IsNullOrWhiteSpace(business.storageItemId);
			}

			if (key.Contains("cash"))
			{
				return !string.IsNullOrWhiteSpace(business.cashDeskItemId);
			}

			if (key.Contains("shelf"))
			{
				return !string.IsNullOrWhiteSpace(business.shelfItemId);
			}

			return !string.IsNullOrWhiteSpace(business.storageItemId) ||
			       !string.IsNullOrWhiteSpace(business.cashDeskItemId) ||
			       !string.IsNullOrWhiteSpace(business.shelfItemId);
		}

		private void SetVisual(bool active)
		{
			if (moduleVisual != null)
			{
				moduleVisual.gameObject.SetActive(active);
			}
		}
	}
}
