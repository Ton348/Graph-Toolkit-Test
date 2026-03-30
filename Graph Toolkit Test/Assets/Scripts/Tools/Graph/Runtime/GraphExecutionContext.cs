using System.Collections.Generic;

public class GraphExecutionContext
{
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    public void Set<T>(GraphContextKey<T> key, T value)
    {
        if (key == null)
        {
            return;
        }

        values[key.Id] = value;
    }

    public bool TryGet<T>(GraphContextKey<T> key, out T value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        return TryGetValue(key.Id, out value);
    }

    public bool Has<T>(GraphContextKey<T> key)
    {
        return key != null && values.ContainsKey(key.Id);
    }

    public bool Remove<T>(GraphContextKey<T> key)
    {
        return key != null && values.Remove(key.Id);
    }

    public void SetValue<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        values[key] = value;
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = default;
            return false;
        }

        if (!values.TryGetValue(key, out object raw))
        {
            value = default;
            return false;
        }

        if (raw == null)
        {
            value = default;
            return true;
        }

        if (raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public bool HasValue(string key)
    {
        return !string.IsNullOrEmpty(key) && values.ContainsKey(key);
    }

    public bool RemoveValue(string key)
    {
        return !string.IsNullOrEmpty(key) && values.Remove(key);
    }

    public void Clear()
    {
        values.Clear();
    }
}
