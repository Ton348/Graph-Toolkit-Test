using System.Collections.Generic;
using UnityEngine;

public class CompassUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CompassManager compassManager;
    [SerializeField] private RectTransform compassBar;
    [SerializeField] private Transform markersContainer;
    [SerializeField] private CompassMarkerView markerPrefab;

    [Header("Settings")]
    [SerializeField] private float maxVisibleAngle = 90f;

    private readonly Dictionary<string, CompassMarkerView> _markers = new Dictionary<string, CompassMarkerView>();
    private readonly HashSet<string> _activeIds = new HashSet<string>();
    private readonly List<string> _toRemove = new List<string>();

    private float _halfWidth;

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

        CacheHalfWidth();
    }

    private void OnEnable()
    {
        if (compassManager != null)
        {
            compassManager.ActiveTargetsChanged += SyncMarkers;
        }

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
}
