using System;

public class GraphContextKey
{
    public string Id { get; }
    public Type ValueType { get; }

    public GraphContextKey(string id, Type valueType)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("GraphContextKey id cannot be null or empty.", nameof(id));
        }

        Id = id;
        ValueType = valueType ?? typeof(object);
    }

    public override string ToString()
    {
        return $"{Id}<{ValueType.Name}>";
    }
}

public sealed class GraphContextKey<T> : GraphContextKey
{
    public GraphContextKey(string id)
        : base(id, typeof(T))
    {
    }
}
