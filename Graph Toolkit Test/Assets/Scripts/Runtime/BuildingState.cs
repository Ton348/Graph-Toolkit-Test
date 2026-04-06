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
        CurrentIncome = 0;
        CurrentExpenses = 0;
    }
}
