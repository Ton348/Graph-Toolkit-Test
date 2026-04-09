using System;
using System.Collections.Generic;

public sealed class GraphNodeExecutorRegistry
{
    private readonly Dictionary<Type, IGraphNodeExecutor> m_registeredExecutors = new Dictionary<Type, IGraphNodeExecutor>();
    private readonly Dictionary<Type, IGraphNodeExecutor> m_resolvedExecutorCache = new Dictionary<Type, IGraphNodeExecutor>();
    private readonly HashSet<Type> m_missingExecutorCache = new HashSet<Type>();

    public GraphNodeExecutorRegistry(IEnumerable<IGraphNodeExecutor> executors)
    {
        if (executors == null)
        {
            throw new ArgumentNullException(nameof(executors));
        }

        foreach (IGraphNodeExecutor executor in executors)
        {
            Register(executor);
        }
    }

    public void Register(IGraphNodeExecutor executor)
    {
        if (executor == null)
        {
            throw new ArgumentNullException(nameof(executor));
        }

        if (m_registeredExecutors.TryGetValue(executor.NodeType, out IGraphNodeExecutor existing))
        {
            throw new InvalidOperationException($"Executor for node type '{executor.NodeType.Name}' is already registered by '{existing.GetType().Name}'.");
        }

        m_registeredExecutors.Add(executor.NodeType, executor);
        m_resolvedExecutorCache.Clear();
        m_missingExecutorCache.Clear();
    }

    public bool Unregister(Type nodeType)
    {
        if (nodeType == null)
        {
            throw new ArgumentNullException(nameof(nodeType));
        }

        bool removed = m_registeredExecutors.Remove(nodeType);
        if (removed)
        {
            m_resolvedExecutorCache.Clear();
            m_missingExecutorCache.Clear();
        }

        return removed;
    }

    public void Clear()
    {
        m_registeredExecutors.Clear();
        m_resolvedExecutorCache.Clear();
        m_missingExecutorCache.Clear();
    }

    public bool TryGetExecutor(BaseGraphNode node, out IGraphNodeExecutor executor)
    {
        executor = null;
        if (node == null)
        {
            return false;
        }

        Type runtimeType = node.GetType();
        if (m_resolvedExecutorCache.TryGetValue(runtimeType, out executor))
        {
            return true;
        }

        if (m_missingExecutorCache.Contains(runtimeType))
        {
            return false;
        }

        Type cursor = runtimeType;
        while (cursor != null && cursor != typeof(object))
        {
            if (m_registeredExecutors.TryGetValue(cursor, out executor))
            {
                m_resolvedExecutorCache[runtimeType] = executor;
                return true;
            }

            cursor = cursor.BaseType;
        }

        m_missingExecutorCache.Add(runtimeType);
        return false;
    }
}
