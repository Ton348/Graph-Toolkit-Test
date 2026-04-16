public class BuildingState
{
    public BuildingDefinitionData definition;
    public bool isOwned;
    public int level;
    public int currentIncome;
    public int currentExpenses;

    public BuildingState(BuildingDefinitionData definition)
    {
        this.definition = definition;
        isOwned = false;
        level = 0;
        currentIncome = 0;
        currentExpenses = 0;
    }
}
