using UnityEngine;

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

        if (bootstrap.PlayerStateSync != null && bootstrap.PlayerStateSync.IsBuildingOwned(buildingId))
        {
            Debug.LogWarning($"[BuildingInteractable] Upgrade is not supported by server yet. BuildingId='{buildingId}'");
            return;
        }

        TryBuyBuilding(buildingId);
    }

    public override void Interact()
    {
        Interact(null);
    }

    private async void TryBuyBuilding(string buildingId)
    {
        if (bootstrap == null || bootstrap.GameServer == null)
        {
            return;
        }

        if (bootstrap.RequestManager != null && !bootstrap.RequestManager.TryStartRequest("BuyBuildingInteractable"))
        {
            return;
        }

        var result = await bootstrap.GameServer.TryBuyBuildingAsync(buildingId);
        if (result != null && result.ProfileSnapshot != null)
        {
            bootstrap.ProfileSyncService?.ApplySnapshot(result.ProfileSnapshot);
        }

        bootstrap.RequestManager?.FinishRequest();
    }
}
