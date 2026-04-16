using System;

public class RequestManager
{
    public bool IsRequestInProgress { get; private set; }
    public string ActiveRequestLabel { get; private set; }

    public event Action<string> requestStarted;
    public event Action<string> requestFinished;
    public event Action<bool> requestStateChanged;

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
        requestStarted?.Invoke(label);
        requestStateChanged?.Invoke(true);
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
        requestFinished?.Invoke(label);
        requestStateChanged?.Invoke(false);
    }
}
