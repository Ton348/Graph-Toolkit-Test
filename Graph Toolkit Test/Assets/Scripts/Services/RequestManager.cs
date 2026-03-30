using System;

public class RequestManager
{
    public bool IsRequestInProgress { get; private set; }
    public string ActiveRequestLabel { get; private set; }

    public event Action<string> RequestStarted;
    public event Action<string> RequestFinished;
    public event Action<bool> RequestStateChanged;

    public bool TryStartRequest(string label)
    {
        if (IsRequestInProgress)
        {
            UnityEngine.Debug.Log($"[RequestManager] Request BLOCKED '{label}' (active='{ActiveRequestLabel}')");
            return false;
        }

        IsRequestInProgress = true;
        ActiveRequestLabel = label;
        UnityEngine.Debug.Log($"[RequestManager] Request START '{label}'");
        RequestStarted?.Invoke(label);
        RequestStateChanged?.Invoke(true);
        return true;
    }

    public void FinishRequest()
    {
        if (!IsRequestInProgress)
        {
            return;
        }

        string label = ActiveRequestLabel;
        IsRequestInProgress = false;
        ActiveRequestLabel = null;
        UnityEngine.Debug.Log($"[RequestManager] Request FINISH '{label}'");
        RequestFinished?.Invoke(label);
        RequestStateChanged?.Invoke(false);
    }
}
