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
            return;
        }

        if (bootstrap == null)
        {
            return;
        }

        if (state == null)
        {
            state = bootstrap.GetBuildingState(definition);
        }

        if (state == null)
        {
            return;
        }

        if (!state.IsOwned)
        {
            if (!bootstrap.PlayerService.HasEnoughMoney(definition.purchaseCost))
            {
                return;
            }

            bootstrap.BuildingService.TryBuyBuilding(state, bootstrap.RuntimeState.Player);
            return;
        }

        bootstrap.BuildingService.TryUpgradeBuilding(state, bootstrap.RuntimeState.Player);
    }

    public override void Interact()
    {
        Interact(null);
    }
}
