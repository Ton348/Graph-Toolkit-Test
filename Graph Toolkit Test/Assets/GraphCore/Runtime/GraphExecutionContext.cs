using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public interface IGraphDialogueService
{
    UniTask ShowAsync(string title, string body, CancellationToken cancellationToken);
    void EndConversation();
}

public readonly struct GraphChoiceEntry
{
    public readonly string label;

    public GraphChoiceEntry(string label)
    {
        this.label = label;
    }
}

public interface IGraphChoiceService
{
    UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken);
}

public interface IGraphMapMarkerService
{
    void ShowOrUpdateMarker(string markerId, string targetObjectName);
}

public interface IGraphCutsceneService
{
    UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
}

public interface IGraphCheckpointService
{
    UniTask<bool> SaveAsync(string checkpointId, CancellationToken cancellationToken);
    UniTask<bool> ClearAsync(string checkpointId, CancellationToken cancellationToken);
}

public interface IGraphQuestService
{
    UniTask<bool> StartQuestAsync(string questId, CancellationToken cancellationToken);
    UniTask<bool> CompleteQuestAsync(string questId, CancellationToken cancellationToken);
    UniTask<QuestState> GetQuestStateAsync(string questId, CancellationToken cancellationToken);
}

public interface IGraphRuntimeServices
{
    IGraphDialogueService dialogueService { get; }
    IGraphChoiceService choiceService { get; }
    IGraphMapMarkerService mapMarkerService { get; }
    IGraphCutsceneService cutsceneService { get; }
    IGraphCheckpointService checkpointService { get; }
    IGraphQuestService questService { get; }
}

public sealed class GraphRuntimeServices : IGraphRuntimeServices
{
    public IGraphDialogueService dialogueService { get; }
    public IGraphChoiceService choiceService { get; }
    public IGraphMapMarkerService mapMarkerService { get; }
    public IGraphCutsceneService cutsceneService { get; }
    public IGraphCheckpointService checkpointService { get; }
    public IGraphQuestService questService { get; }

    public GraphRuntimeServices(
        IGraphDialogueService dialogueService,
        IGraphChoiceService choiceService,
        IGraphMapMarkerService mapMarkerService,
        IGraphCutsceneService cutsceneService,
        IGraphCheckpointService checkpointService,
        IGraphQuestService questService)
    {
        this.dialogueService = dialogueService;
        this.choiceService = choiceService;
        this.mapMarkerService = mapMarkerService;
        this.cutsceneService = cutsceneService;
        this.checkpointService = checkpointService;
        this.questService = questService;
    }
}

public sealed class GraphExecutionContext
{
    private sealed class TypedValue
    {
        public readonly Type valueType;
        public readonly object value;

        public TypedValue(Type valueType, object value)
        {
            this.valueType = valueType ?? typeof(object);
            this.value = value;
        }
    }

    private readonly Dictionary<GraphContextKey, TypedValue> m_values = new Dictionary<GraphContextKey, TypedValue>();

    public GraphExecutionContext(IGraphRuntimeServices services = null)
    {
        Services = services;
    }

    public IGraphRuntimeServices Services { get; }

    public IGraphDialogueService DialogueService => Services?.dialogueService;
    public IGraphChoiceService ChoiceService => Services?.choiceService;
    public IGraphMapMarkerService MapMarkerService => Services?.mapMarkerService;
    public IGraphCutsceneService CutsceneService => Services?.cutsceneService;
    public IGraphCheckpointService CheckpointService => Services?.checkpointService;
    public IGraphQuestService QuestService => Services?.questService;

    public void Set<T>(GraphContextKey<T> key, T value)
    {
        EnsureKeyIsValid(key);
        SetInternal(key, value);
    }

    public bool TryGet<T>(GraphContextKey<T> key, out T value)
    {
        EnsureKeyIsValid(key);
        return TryGetInternal(key, out value);
    }

    public bool Contains<T>(GraphContextKey<T> key)
    {
        EnsureKeyIsValid(key);
        return m_values.ContainsKey(key);
    }

    public bool Remove<T>(GraphContextKey<T> key)
    {
        EnsureKeyIsValid(key);
        return m_values.Remove(key);
    }

    public void Clear()
    {
        m_values.Clear();
    }

    private static void EnsureKeyIsValid<T>(GraphContextKey<T> key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
    }

    private void SetInternal<T>(GraphContextKey<T> key, T value)
    {
        if (m_values.TryGetValue(key, out TypedValue existing) && existing.valueType != key.ValueType)
        {
            throw new InvalidOperationException($"GraphExecutionContext type mismatch on key '{key.Id}'. Existing type: {existing.valueType.Name}, new type: {key.ValueType.Name}.");
        }

        m_values[key] = new TypedValue(key.ValueType, value);
    }

    private bool TryGetInternal<T>(GraphContextKey<T> key, out T value)
    {
        if (!m_values.TryGetValue(key, out TypedValue stored))
        {
            value = default;
            return false;
        }

        if (stored.valueType != key.ValueType)
        {
            throw new InvalidOperationException($"GraphExecutionContext type mismatch on key '{key.Id}'. Stored type: {stored.valueType.Name}, requested type: {key.ValueType.Name}.");
        }

        if (stored.value == null)
        {
            value = default;
            return true;
        }

        if (stored.value is not T typedValue)
        {
            throw new InvalidOperationException($"GraphExecutionContext stored value cast failed on key '{key.Id}'. Stored runtime type: {stored.value.GetType().Name}, requested type: {key.ValueType.Name}.");
        }

        value = typedValue;
        return true;
    }
}
