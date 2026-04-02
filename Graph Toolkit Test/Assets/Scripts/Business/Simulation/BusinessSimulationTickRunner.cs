using UnityEngine;

public class BusinessSimulationTickRunner : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public float tickIntervalSeconds = 1f;
    public float timeScale = 1f;

    private BusinessSimulationService simulationService;
    private float accumulator;

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
    }

    private void Update()
    {
        if (simulationService == null || tickIntervalSeconds <= 0f)
        {
            return;
        }

        simulationService.TimeScale = timeScale;
        accumulator += Time.deltaTime;

        while (accumulator >= tickIntervalSeconds)
        {
            simulationService.Tick(tickIntervalSeconds);
            accumulator -= tickIntervalSeconds;
        }
    }
}
