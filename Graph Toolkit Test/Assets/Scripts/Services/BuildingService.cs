[System.Obsolete("Legacy client runtime. Only LocalGameServer uses this service.")]
public class BuildingService
{
    public BuildingService()
    {
    }

    public bool TryBuyBuilding(BuildingState building, PlayerProfileState player)
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

    public UpgradeResult TryUpgradeBuilding(BuildingState building, PlayerProfileState player)
    {
        if (building == null || player == null || building.Definition == null)
        {
            return UpgradeResult.NotOwned;
        }

        if (!building.IsOwned)
        {
            return UpgradeResult.NotOwned;
        }

        // Legacy upgrade flow is disabled while buildings are used only for
        // purchase + persistent visual construction.
        return UpgradeResult.NotOwned;
    }
}
