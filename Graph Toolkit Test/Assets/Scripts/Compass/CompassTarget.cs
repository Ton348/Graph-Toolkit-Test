using UnityEngine;

public class CompassTarget : MonoBehaviour
{
    [SerializeField] private string targetId;
    [SerializeField] private Transform markerPoint;
    private bool _registered;

    public string TargetId => targetId;

    public Vector3 GetMarkerWorldPosition()
    {
        return markerPoint != null ? markerPoint.position : transform.position;
    }

    private void OnEnable()
    {
        TryRegister();
    }

    private void Start()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        TryUnregister();
    }

    private void TryRegister()
    {
        if (_registered) return;

        var registry = CompassTargetRegistry.Instance;
        if (registry == null)
        {
            registry = FindObjectOfType<CompassTargetRegistry>();
        }

        if (registry == null) return;

        registry.Register(this);
        _registered = true;
    }

    private void TryUnregister()
    {
        if (!_registered) return;

        var registry = CompassTargetRegistry.Instance;
        if (registry == null)
        {
            registry = FindObjectOfType<CompassTargetRegistry>();
        }

        if (registry == null) return;

        registry.Unregister(this);
        _registered = false;
    }
}
