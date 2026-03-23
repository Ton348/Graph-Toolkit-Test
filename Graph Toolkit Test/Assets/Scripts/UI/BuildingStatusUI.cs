using TMPro;
using UnityEngine;

public class BuildingStatusUI : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public BuildingDefinition buildingDefinition;
    public TMP_Text statusText;

    private void Update()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (statusText == null || bootstrap == null || buildingDefinition == null)
        {
            return;
        }

        BuildingState state = bootstrap.GetBuildingState(buildingDefinition);
        if (state == null || state.Definition == null)
        {
            statusText.text = "Building: not found";
            return;
        }

        string displayName = string.IsNullOrEmpty(state.Definition.displayName) ? state.Definition.name : state.Definition.displayName;

        statusText.text =
            $"Building: {displayName}\n" +
            $"Owned: {state.IsOwned}\n" +
            $"Level: {state.Level}\n" +
            $"Income: {state.CurrentIncome}\n" +
            $"Expenses: {state.CurrentExpenses}";
    }
}
