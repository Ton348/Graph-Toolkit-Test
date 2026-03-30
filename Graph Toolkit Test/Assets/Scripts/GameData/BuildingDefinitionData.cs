using System;

[Serializable]
public class BuildingDefinitionData
{
    public string id;
    public string displayName;
    public int purchaseCost;
    public int rentPerMonth;
    public int incomePerDay;
    public int expensesPerDay;
    public int netProfit;
    public int upgradeCost;
    public int upgradeIncomeBonus;
}
