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
			// replace existing
			for (int i = 0; i < m_converters.Count; i++)
			{
				if (m_converters[i].GetType() == converterType)
				{
					m_converters[i] = converter;
					return;
				}
			}
		}

		m_converters.Add(converter);
		m_registeredConverterTypes.Add(converterType);
	}

	public void Register<TConverter>() where TConverter : IGameGraphNodeConverter, new()
	{
		Register(new TConverter());
	}

	public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
	{
		if (editorNodeModel == null)
		{
			runtimeNode = null;
			return false;
		}

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

	public bool HasConverterFor(object editorNodeModel)
	{
		if (editorNodeModel == null)
		{
			return false;
		}

		for (int i = 0; i < m_converters.Count; i++)
		{
			if (m_converters[i].CanConvert(editorNodeModel))
			{
				return true;
			}
		}

		return false;
	}

	public IReadOnlyList<IGameGraphNodeConverter> GetConverters()
	{
		return m_converters.AsReadOnly();
	}
}
