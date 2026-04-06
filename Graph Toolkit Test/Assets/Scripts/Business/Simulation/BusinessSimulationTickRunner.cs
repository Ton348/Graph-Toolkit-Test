using UnityEngine;

public class BusinessSimulationTickRunner : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public float tickIntervalSeconds = 1f;
    public float timeScale = 1f;

    private BusinessSimulationService simulationService;
    private BusinessSimulationClock clock;

    private void Awake()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (bootstrap != null)
        {
            simulationService = bootstrap.BusinessSimulationService;
        }

        clock = new BusinessSimulationClock
        {
            tickIntervalSeconds = tickIntervalSeconds
        };
    }

    private void Update()
    {
        if (simulationService == null || clock == null)
        {
            return;
        }

        clock.tickIntervalSeconds = tickIntervalSeconds;
        simulationService.TimeScale = timeScale;
        clock.Update(Time.deltaTime);
        while (clock.TryConsumeTick(out var delta))
        {
            simulationService.RunTick(delta);
        }
    }
}
