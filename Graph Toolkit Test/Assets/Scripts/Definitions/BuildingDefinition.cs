using UnityEngine;

[CreateAssetMenu(menuName = "Game/Definitions/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    public string buildingId;
    public string displayName;
    public int purchaseCost;
    public int rentPerMonth;
    public int incomePerDay;
    public int expensesPerDay;
    public int netProfit;
    public int upgradeCost;
    public int upgradeIncomeBonus;
}
