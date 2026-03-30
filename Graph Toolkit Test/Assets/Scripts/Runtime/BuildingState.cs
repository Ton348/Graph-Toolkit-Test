public class BuildingState
{
    public BuildingDefinitionData Definition;
    public bool IsOwned;
    public int Level;
    public int CurrentIncome;
    public int CurrentExpenses;

    public BuildingState(BuildingDefinitionData definition)
    {
        Definition = definition;
        IsOwned = false;
        Level = 0;
        CurrentIncome = definition != null ? definition.incomePerDay : 0;
        CurrentExpenses = definition != null ? definition.expensesPerDay : 0;
    }
}
