using UnityEngine;

public class GraphDebugOverlay : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public int maxLines = 12;
    public Vector2 position = new Vector2(10, 10);
    public Vector2 size = new Vector2(520, 260);

    private GraphDebugService debugService;

    private void Awake()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        debugService = bootstrap != null ? bootstrap.GraphDebugService : null;
    }

    private void OnGUI()
    {
        if (debugService == null || !debugService.Enabled)
        {
            return;
        }

        Rect rect = new Rect(position.x, position.y, size.x, size.y);
        GUILayout.BeginArea(rect, GUI.skin.box);
        GUILayout.Label("Graph Debug");

        var history = debugService.History;
        int start = Mathf.Max(0, history.Count - maxLines);
        for (int i = start; i < history.Count; i++)
        {
            GraphExecutionEvent evt = history[i];
            string line = $"{evt.EventType} | {evt.NodeName}";
            if (!string.IsNullOrEmpty(evt.Message))
            {
                line += $" | {evt.Message}";
            }
            GUILayout.Label(line);
        }

        GUILayout.EndArea();
    }
}
