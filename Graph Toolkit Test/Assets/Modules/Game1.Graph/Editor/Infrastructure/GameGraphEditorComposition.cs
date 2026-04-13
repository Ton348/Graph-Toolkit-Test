using System;
using System.Collections.Generic;

public sealed class GameGraphEditorComposition
{
	private readonly GameGraphNodeConverterRegistry m_converterRegistry;

	public GameGraphEditorComposition(GameGraphNodeConverterRegistry converterRegistry)
	{
		m_converterRegistry = converterRegistry ?? throw new ArgumentNullException(nameof(converterRegistry));
	}

	public GameGraphNodeConverterRegistry ConverterRegistry => m_converterRegistry;

	public void RegisterConverter(IGameGraphNodeConverter converter)
	{
		m_converterRegistry.Register(converter);
	}

	public void RegisterConverters(IEnumerable<IGameGraphNodeConverter> converters)
	{
		if (converters == null)
		{
			return;
		}

		foreach (IGameGraphNodeConverter converter in converters)
		{
			if (converter == null)
			{
				continue;
			}

			m_converterRegistry.Register(converter);
		}
	}

	public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
	{
		return m_converterRegistry.TryConvert(editorNodeModel, out runtimeNode);
	}

	public static GameGraphEditorComposition CreateDefault()
	{
		return new GameGraphEditorComposition(new GameGraphNodeConverterRegistry());
	}
}
