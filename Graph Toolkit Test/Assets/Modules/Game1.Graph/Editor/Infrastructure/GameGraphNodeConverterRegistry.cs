using System;
using System.Collections.Generic;

public sealed class GameGraphNodeConverterRegistry
{
	private readonly List<IGameGraphNodeConverter> m_converters = new List<IGameGraphNodeConverter>();
	private readonly HashSet<Type> m_registeredConverterTypes = new HashSet<Type>();

	public void Register(IGameGraphNodeConverter converter)
	{
		if (converter == null)
		{
			throw new ArgumentNullException(nameof(converter));
		}

		Type converterType = converter.GetType();
		if (m_registeredConverterTypes.Contains(converterType))
		{
			throw new InvalidOperationException($"Converter '{converterType.Name}' is already registered.");
		}

		m_converters.Add(converter);
		m_registeredConverterTypes.Add(converterType);
	}

	public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
	{
		for (int i = 0; i < m_converters.Count; i++)
		{
			IGameGraphNodeConverter converter = m_converters[i];
			if (!converter.CanConvert(editorNodeModel))
			{
				continue;
			}

			if (converter.TryConvert(editorNodeModel, out runtimeNode) && runtimeNode != null)
			{
				return true;
			}
		}

		runtimeNode = null;
		return false;
	}
}
