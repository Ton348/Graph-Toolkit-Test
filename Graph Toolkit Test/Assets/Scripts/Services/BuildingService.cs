public class BuildingService
{
    private readonly EventBus eventBus;

    public BuildingService(EventBus eventBus)
    {
        this.eventBus = eventBus;
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
        eventBus?.Publish(new BuildingPurchasedEvent(building));
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

        eventBus?.Publish(new BuildingUpgradedEvent(building));
        return UpgradeResult.Success;
    }
}
