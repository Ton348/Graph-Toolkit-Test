using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public sealed class GraphExecutionContext
{
	private readonly Dictionary<GraphContextKey, (Type valueType, object value)> m_values = new Dictionary<GraphContextKey, (Type valueType, object value)>();

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
		if (m_values.TryGetValue(key, out (Type valueType, object value) existing) && existing.valueType != key.ValueType)
		{
			throw new InvalidOperationException($"GraphExecutionContext type mismatch on key '{key.Id}'. Existing type: {existing.valueType.Name}, new type: {key.ValueType.Name}.");
		}

		m_values[key] = (key.ValueType, value);
	}

	private bool TryGetInternal<T>(GraphContextKey<T> key, out T value)
	{
		if (!m_values.TryGetValue(key, out (Type valueType, object value) stored))
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
