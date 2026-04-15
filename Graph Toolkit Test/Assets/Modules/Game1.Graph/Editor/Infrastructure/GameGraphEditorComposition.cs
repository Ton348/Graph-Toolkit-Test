using Game1.Graph.Runtime;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Game1.Graph.Editor
{
	public sealed class GameGraphEditorComposition
	{
		public GameGraphNodeConverterRegistry ConverterRegistry { get; }

		public GameGraphEditorComposition(GameGraphNodeConverterRegistry converterRegistry)
		{
			ConverterRegistry = converterRegistry ?? throw new ArgumentNullException(nameof(converterRegistry));
		}

		public void RegisterConverter(IGameGraphNodeConverter converter)
		{
			ConverterRegistry.Register(converter);
		}

		public void RegisterConverter<TConverter>() where TConverter : IGameGraphNodeConverter, new()
		{
			ConverterRegistry.Register<TConverter>();
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

				ConverterRegistry.Register(converter);
			}
		}

		public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
		{
			return ConverterRegistry.TryConvert(editorNodeModel, out runtimeNode);
		}

		public void RegisterConvertersFromAssemblies(IEnumerable<Assembly> assemblies)
		{
			GameGraphAutoRegistration.RegisterConverters(ConverterRegistry, assemblies);
		}

		public static GameGraphEditorComposition CreateDefault()
		{
			return new GameGraphEditorComposition(new GameGraphNodeConverterRegistry());
		}
	}
}
