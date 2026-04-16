using TMPro;
using UnityEngine;

public class BusinessShelfVisual : MonoBehaviour
{
    public BusinessWorldRuntime worldRuntime;
    public TMP_Text valueText;
    public Transform fillTransform;
    public Vector3 minScale = new Vector3(1f, 0.05f, 1f);
    public Vector3 maxScale = new Vector3(1f, 1f, 1f);

    private BusinessSimulationService m_simulation;

    private void OnEnable()
    {
        ResolveSimulation();
        Refresh();
    }

    private void OnDisable()
    {
        if (m_simulation != null)
        {
            m_simulation.simulationUpdated -= Refresh;
        }
    }

    private void ResolveSimulation()
    {
        if (worldRuntime == null)
        {
            worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
        }

        m_simulation = worldRuntime != null ? worldRuntime.GetSimulationService() : null;
        if (m_simulation != null)
        {
            m_simulation.simulationUpdated += Refresh;
        }
    }

    private void Refresh()
    {
        if (m_simulation == null || worldRuntime == null)
        {
            return;
        }

        var state = m_simulation.GetStateByLotId(worldRuntime.lotId);
        if (state == null)
        {
            return;
        }

        float capacity = Mathf.Max(1f, state.shelfCapacity);
        float ratio = Mathf.Clamp01(state.shelfStock / capacity);

        if (fillTransform != null)
        {
            fillTransform.localScale = Vector3.Lerp(minScale, maxScale, ratio);
        }

        if (valueText != null)
        {
            valueText.text = $"{state.shelfStock:0.##}/{capacity:0}";
        }
    }
}
