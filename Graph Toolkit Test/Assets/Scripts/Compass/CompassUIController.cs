using System.Collections.Generic;
using UnityEngine;

public class CompassUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CompassManager compassManager;
    [SerializeField] private RectTransform compassBar;
    [SerializeField] private Transform markersContainer;
    [SerializeField] private CompassMarkerView markerPrefab;
    [SerializeField] private Transform ticksContainer;
    [SerializeField] private CompassTickView tickPrefab;

    [Header("Settings")]
    [SerializeField] private float maxVisibleAngle = 90f;
    [SerializeField] private List<TickDefinition> tickDefinitions = new List<TickDefinition>();

    private readonly Dictionary<string, CompassMarkerView> _markers = new Dictionary<string, CompassMarkerView>();
    private readonly HashSet<string> _activeIds = new HashSet<string>();
    private readonly List<string> _toRemove = new List<string>();
    private readonly List<TickRuntime> _ticks = new List<TickRuntime>();

    private float _halfWidth;

    [System.Serializable]
    public struct TickDefinition
    {
        public string label;
        [Range(-180f, 180f)] public float worldYaw;
    }

    private class TickRuntime
    {
        public TickDefinition def;
        public CompassTickView view;
    }

    private void Awake()
    {
        if (compassManager == null)
        {
            compassManager = CompassManager.Instance;
        }

        if (compassBar == null)
        {
            compassBar = GetComponent<RectTransform>();
        }

        if (markersContainer == null && compassBar != null)
        {
            markersContainer = compassBar;
        }

        if (ticksContainer == null && compassBar != null)
        {
            ticksContainer = compassBar;
        }

        if (!IsSceneTransform(ticksContainer))
        {
            ticksContainer = IsSceneTransform(compassBar) ? compassBar : transform;
        }

        EnsureDefaultTicks();
        CacheHalfWidth();
    }

    private void OnEnable()
    {
        if (compassManager != null)
        {
            compassManager.ActiveTargetsChanged += SyncMarkers;
        }

        EnsureTicks();
        SyncMarkers();
    }

    private void OnDisable()
    {
        if (compassManager != null)
        {
            compassManager.ActiveTargetsChanged -= SyncMarkers;
        }
    }

    private void LateUpdate()
    {
        UpdateMarkers();
        UpdateTicks();
    }

    private void OnRectTransformDimensionsChange()
    {
        CacheHalfWidth();
    }

    private void CacheHalfWidth()
    {
        if (compassBar != null)
        {
            _halfWidth = compassBar.rect.width * 0.5f;
        }
    }

    private void SyncMarkers()
    {
        if (compassManager == null || markerPrefab == null) return;

        _activeIds.Clear();

        foreach (var target in compassManager.ActiveTargets)
        {
            if (target == null) continue;

            string id = target.TargetId;
            if (string.IsNullOrWhiteSpace(id)) continue;

            _activeIds.Add(id);

            if (!_markers.TryGetValue(id, out var view) || view == null)
            {
                var instance = Instantiate(markerPrefab, markersContainer);
                _markers[id] = instance;
            }
        }

        _toRemove.Clear();
        foreach (var kvp in _markers)
        {
            if (!_activeIds.Contains(kvp.Key))
            {
                _toRemove.Add(kvp.Key);
            }
        }

        for (int i = 0; i < _toRemove.Count; i++)
        {
            string id = _toRemove[i];
            if (_markers.TryGetValue(id, out var view) && view != null)
            {
                Destroy(view.gameObject);
            }
            _markers.Remove(id);
        }
    }

    private void EnsureDefaultTicks()
    {
        if (tickDefinitions != null && tickDefinitions.Count > 0) return;

        tickDefinitions = new List<TickDefinition>
        {
            new TickDefinition { label = "N", worldYaw = 0f },
            new TickDefinition { label = "E", worldYaw = 90f },
            new TickDefinition { label = "S", worldYaw = 180f },
            new TickDefinition { label = "W", worldYaw = -90f }
        };
    }

    private void EnsureTicks()
    {
        if (tickPrefab == null || ticksContainer == null) return;
        if (_ticks.Count > 0) return;

        if (!IsSceneTransform(ticksContainer))
        {
            ticksContainer = IsSceneTransform(compassBar) ? compassBar : transform;
        }

        EnsureDefaultTicks();

        for (int i = 0; i < tickDefinitions.Count; i++)
        {
            var instance = Instantiate(tickPrefab, ticksContainer);
            instance.SetLabel(tickDefinitions[i].label);
            _ticks.Add(new TickRuntime { def = tickDefinitions[i], view = instance });
        }
    }

    private static bool IsSceneTransform(Transform t)
    {
        return t != null && t.gameObject.scene.IsValid();
    }

    private void UpdateMarkers()
    {
        if (compassManager == null || compassManager.Player == null) return;

        Vector3 playerPos = compassManager.Player.position;
        Vector3 playerForward = compassManager.Player.forward;
        playerForward.y = 0f;
        if (playerForward.sqrMagnitude < 0.0001f)
        {
            playerForward = Vector3.forward;
        }

        foreach (var target in compassManager.ActiveTargets)
        {
            if (target == null) continue;

            string id = target.TargetId;
            if (!_markers.TryGetValue(id, out var view) || view == null) continue;

            Vector3 direction = target.GetMarkerWorldPosition() - playerPos;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                view.SetVisible(true);
                view.SetPositionX(0f);
                continue;
            }

            float angle = Vector3.SignedAngle(playerForward, direction, Vector3.up);

            float normalized;
            if (Mathf.Abs(angle) > maxVisibleAngle)
            {
                normalized = Mathf.Sign(angle);
            }
            else
            {
                normalized = Mathf.Clamp(angle / maxVisibleAngle, -1f, 1f);
            }
            float x = normalized * _halfWidth;

            view.SetVisible(true);
            view.SetPositionX(x);
        }
    }

    private void UpdateTicks()
    {
        if (compassManager == null || compassManager.Player == null) return;
        if (_ticks.Count == 0) return;

        Vector3 forward = compassManager.Player.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        float playerYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

        for (int i = 0; i < _ticks.Count; i++)
        {
            var tick = _ticks[i];
            if (tick.view == null) continue;

            float angle = Mathf.DeltaAngle(playerYaw, tick.def.worldYaw);
            if (Mathf.Abs(angle) > maxVisibleAngle)
            {
                tick.view.SetVisible(false);
                continue;
            }

            float normalized = Mathf.Clamp(angle / maxVisibleAngle, -1f, 1f);
            float x = normalized * _halfWidth;

            tick.view.SetVisible(true);
            tick.view.SetPositionX(x);
        }
    }
}
