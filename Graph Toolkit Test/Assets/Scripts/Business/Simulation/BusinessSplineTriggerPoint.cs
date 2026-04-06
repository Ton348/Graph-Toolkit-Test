using UnityEngine;

public class BusinessSplineTriggerPoint : MonoBehaviour
{
    public enum TriggerKind
    {
        Storage,
        Shelves
    }

    public TriggerKind triggerKind;
    public BusinessWorkerSplineRuntime runtime;

    private void Awake()
    {
        if (runtime == null)
        {
            runtime = GetComponentInParent<BusinessWorkerSplineRuntime>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (runtime == null)
        {
            return;
        }

        var agent = other.GetComponentInParent<BusinessWorkerSplineAgent>() ?? other.GetComponent<BusinessWorkerSplineAgent>();
        if (agent == null)
        {
            return;
        }

        runtime.OnTriggerEntered(this, agent);
    }
}
