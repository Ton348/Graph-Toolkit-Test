using Prototype.Business.Runtime;
using Prototype.Business.Services;
using UnityEngine;

namespace Prototype.Business.World
{
	public class StorageSlot : MonoBehaviour
	{
		[SerializeField] private string requiredItemId = "goods_basic";
		[SerializeField] private int defaultAmount = 1;
		[SerializeField] private KeyCode interactKey = KeyCode.E;
		[SerializeField] private string playerTag = "Player";

		public BusinessWorldRuntime worldRuntime;

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

			Debug.Log($"[StorageSlot] E pressed in trigger. item='{requiredItemId}', lotId='{worldRuntime?.lotId}'");
			TryStoreAsync();
		}

		private async void TryStoreAsync()
		{
			if (string.IsNullOrWhiteSpace(requiredItemId))
			{
				Debug.LogWarning("[StorageSlot] requiredItemId is empty.");
				return;
			}

			if (worldRuntime == null)
			{
				worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
			}

			if (worldRuntime == null)
			{
				Debug.LogWarning("[StorageSlot] worldRuntime is missing.");
				return;
			}

			BusinessActionFacade facade = worldRuntime.bootstrap != null ? worldRuntime.bootstrap.BusinessActionFacade : null;
			BusinessInstanceSnapshot business = worldRuntime.GetBusiness();
			if (facade == null || business == null)
			{
				Debug.LogWarning($"[StorageSlot] facade or business missing. facade={(facade != null)} business={(business != null)} lotId='{worldRuntime.lotId}'");
				return;
			}

			int amount = Mathf.Max(1, defaultAmount);
			Debug.Log($"[StorageSlot] Sending StoreBusinessItem. lotId='{worldRuntime.lotId}', item='{requiredItemId}', amount={amount}, storageBefore={business.storageStock}");

			ServerActionResult result = await facade.StoreBusinessItem(worldRuntime.lotId, requiredItemId, amount);
			if (result == null || !result.Success)
			{
				Debug.LogWarning($"[StorageSlot] StoreBusinessItem failed. resultNull={(result == null)} error='{result?.ErrorCode}' message='{result?.Message}'");
				return;
			}

			BusinessInstanceSnapshot updatedBusiness = worldRuntime.GetBusiness();
			Debug.Log($"[StorageSlot] StoreBusinessItem success. storageAfter={(updatedBusiness != null ? updatedBusiness.storageStock : -1)}");
		}
	}
}
