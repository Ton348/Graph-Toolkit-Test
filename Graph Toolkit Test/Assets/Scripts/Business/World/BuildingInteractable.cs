using Prototype.Business.Bootstrap;
using Prototype.Business.Services;
using Sample.Runtime;
using UnityEngine;

namespace Prototype.Business.World
{
	public class BuildingInteractable : Interactable
	{
		public string buildingId;
		public GameBootstrap bootstrap;

		private void Start()
		{
			if (bootstrap == null)
			{
				bootstrap = FindObjectOfType<GameBootstrap>();
			}
		}

		public override void Interact(Transform player)
		{
			if (bootstrap == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(buildingId))
			{
				return;
			}

			TryBuyBuildingAsync(buildingId);
		}

		public override void Interact()
		{
			Interact(null);
		}

		private async void TryBuyBuildingAsync(string buildingId)
		{
			if (bootstrap == null || bootstrap.GameServer == null)
			{
				return;
			}

			if (bootstrap.RequestManager != null &&
			    !bootstrap.RequestManager.TryStartRequest("BuyBuildingInteractable"))
			{
				return;
			}

			ServerActionResult result = await bootstrap.GameServer.TryBuyBuildingAsync(buildingId);
			if (result != null && result.ProfileSnapshot != null)
			{
				bootstrap.ProfileSyncService?.ApplySnapshot(result.ProfileSnapshot);
			}

			bootstrap.RequestManager?.FinishRequest();
		}
	}
}
