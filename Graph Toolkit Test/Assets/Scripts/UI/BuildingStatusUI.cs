using TMPro;
using UnityEngine;

public class BuildingStatusUI : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public string buildingId;
    public TMP_Text statusText;

    private void Update()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (statusText == null || bootstrap == null || string.IsNullOrEmpty(buildingId) || bootstrap.PlayerStateSync == null || bootstrap.GameDataRepository == null)
        {
            return;
        }

        var def = bootstrap.GameDataRepository.GetBuildingById(buildingId);
        if (def == null)
        {
            statusText.text = "Building: not found";
            return;
        }

        string displayName = string.IsNullOrEmpty(def.displayName) ? def.id : def.displayName;
        bool owned = bootstrap.PlayerStateSync.IsBuildingOwned(def.id);
        int level = 0;
        int income = 0;
        int expenses = 0;
        if (bootstrap.PlayerStateSync.TryGetBuildingState(def.id, out var state))
        {
            level = state.level;
            income = state.currentIncome;
            expenses = state.currentExpenses;
        }

        statusText.text =
            $"Building: {displayName}\n" +
            $"Owned: {owned}\n" +
            $"Level: {level}\n" +
            $"Income: {income}\n" +
            $"Expenses: {expenses}";
    }
}
