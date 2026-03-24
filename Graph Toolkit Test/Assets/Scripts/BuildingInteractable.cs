using UnityEngine;

public class BuildingInteractable : Interactable
{
    public BuildingDefinition definition;
    public GameBootstrap bootstrap;

    private BuildingState state;

    private void Start()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (bootstrap != null && definition != null)
        {
            state = bootstrap.GetBuildingState(definition);
        }
    }

    public override void Interact(Transform player)
    {
        if (definition == null)
        {
            Debug.LogWarning("BuildingDefinition не назначен");
            return;
        }

        if (bootstrap == null)
        {
            Debug.LogWarning("GameBootstrap не найден");
            return;
        }

        if (state == null)
        {
            state = bootstrap.GetBuildingState(definition);
        }

        if (state == null)
        {
            Debug.LogWarning("BuildingState не найден");
            return;
        }

        if (!state.IsOwned)
        {
            if (!bootstrap.PlayerService.HasEnoughMoney(definition.purchaseCost))
            {
                Debug.Log("Недостаточно денег");
                return;
            }

            bool success = bootstrap.BuildingService.TryBuyBuilding(state, bootstrap.RuntimeState.Player);
            if (success)
            {
                Debug.Log("Здание куплено");
            }
            else
            {
                Debug.Log("Здание уже куплено");
            }

            return;
        }

        UpgradeResult upgradeResult = bootstrap.BuildingService.TryUpgradeBuilding(state, bootstrap.RuntimeState.Player);
        if (upgradeResult == UpgradeResult.Success)
        {
            Debug.Log("Здание улучшено");
        }
        else if (upgradeResult == UpgradeResult.NotEnoughMoney)
        {
            Debug.Log("Недостаточно денег для улучшения");
        }
        else
        {
            Debug.Log("Сначала купите здание");
        }
    }

    public override void Interact()
    {
        Interact(null);
    }
}
