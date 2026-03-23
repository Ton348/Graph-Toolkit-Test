using System;
using System.Collections.Generic;

public class EventBus
{
    private readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

    public void Subscribe<T>(Action<T> handler)
    {
        if (handler == null)
        {
            return;
        }

        Type type = typeof(T);
        if (subscribers.TryGetValue(type, out Delegate existing))
        {
            subscribers[type] = Delegate.Combine(existing, handler);
        }
        else
        {
            subscribers[type] = handler;
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler == null)
        {
            return;
        }

        Type type = typeof(T);
        if (!subscribers.TryGetValue(type, out Delegate existing))
        {
            return;
        }

        Delegate current = Delegate.Remove(existing, handler);
        if (current == null)
        {
            subscribers.Remove(type);
        }
        else
        {
            subscribers[type] = current;
        }
    }

    public void Publish<T>(T eventData)
    {
        if (subscribers.TryGetValue(typeof(T), out Delegate existing))
        {
            Action<T> callback = existing as Action<T>;
            callback?.Invoke(eventData);
        }
    }
}
