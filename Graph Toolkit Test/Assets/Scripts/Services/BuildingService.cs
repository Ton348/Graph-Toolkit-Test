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

        int cost = building.Definition.upgradeCost;
        if (player.Money < cost)
        {
            return UpgradeResult.NotEnoughMoney;
        }

        player.Money -= cost;
        building.Level += 1;
        building.CurrentIncome += building.Definition.upgradeIncomeBonus;

        return UpgradeResult.Success;
    }
}
