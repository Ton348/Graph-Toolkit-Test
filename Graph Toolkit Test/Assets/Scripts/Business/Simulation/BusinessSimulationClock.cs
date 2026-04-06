using UnityEngine;

public class BusinessSimulationClock
{
    public float tickIntervalSeconds = 1f;

    private float accumulator;

    public void Update(float deltaTime)
    {
        if (tickIntervalSeconds <= 0f)
        {
            return;
        }

        accumulator += Mathf.Max(0f, deltaTime);
    }

    public bool TryConsumeTick(out float delta)
    {
        delta = 0f;
        if (tickIntervalSeconds <= 0f || accumulator < tickIntervalSeconds)
        {
            return false;
        }

        accumulator -= tickIntervalSeconds;
        delta = tickIntervalSeconds;
        return true;
    }
}
