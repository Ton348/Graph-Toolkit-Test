using Game1.Graph.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace Game1.Graph.Editor
{
	public static class GameGraphAutoRegistration
	{
		public static IReadOnlyList<Assembly> BuildAssemblySet(params Type[] markerTypes)
		{
			if (markerTypes == null || markerTypes.Length == 0)
			{
				return Array.Empty<Assembly>();
			}

			HashSet<Assembly> assemblies = new HashSet<Assembly>();
			for (int i = 0; i < markerTypes.Length; i++)
			{
				Type markerType = markerTypes[i];
				if (markerType?.Assembly == null)
				{
					continue;
				}

				assemblies.Add(markerType.Assembly);
			}

			return assemblies.ToList();
		}

		public static void RegisterConverters(GameGraphNodeConverterRegistry converterRegistry, IEnumerable<Assembly> assemblies)
		{
			if (converterRegistry == null)
			{
				throw new ArgumentNullException(nameof(converterRegistry));
			}

			foreach (Type type in EnumerateTypesWithAttribute<GameGraphNodeConverterAttribute>(assemblies))
			{
				if (!typeof(IGameGraphNodeConverter).IsAssignableFrom(type) || type.IsAbstract || type.IsGenericTypeDefinition)
				{
					continue;
				}

				if (TryCreateInstance(type, out IGameGraphNodeConverter converter))
				{
					converterRegistry.Register(converter);
				}
			}
		}

		public static void RegisterExecutors(GameGraphExecutorRegistry executorRegistry, IEnumerable<Assembly> assemblies)
		{
			if (executorRegistry == null)
			{
				throw new ArgumentNullException(nameof(executorRegistry));
			}

			foreach (Type type in EnumerateTypesWithAttribute<GameGraphNodeExecutorAttribute>(assemblies))
			{
				if (!typeof(IGraphNodeExecutor).IsAssignableFrom(type) || type.IsAbstract || type.IsGenericTypeDefinition)
				{
					continue;
				}

				if (TryCreateInstance(type, out IGraphNodeExecutor executor))
				{
					executorRegistry.Register(executor);
				}
			}
		}

		public static void RegisterValidators(GameGraphNodeValidatorRegistry validatorRegistry, IEnumerable<Assembly> assemblies)
		{
			if (validatorRegistry == null)
			{
				throw new ArgumentNullException(nameof(validatorRegistry));
			}

			foreach (Type type in EnumerateTypesWithAttribute<GameGraphNodeValidatorAttribute>(assemblies))
			{
				if (!typeof(IGameGraphNodeValidator).IsAssignableFrom(type) || type.IsAbstract || type.IsGenericTypeDefinition)
				{
					continue;
				}

				if (TryCreateInstance(type, out IGameGraphNodeValidator validator))
				{
					validatorRegistry.Register(validator);
				}
			}
		}

		public static IReadOnlyList<Type> FindUnsupportedModels(IEnumerable<object> editorNodeModels, GameGraphNodeConverterRegistry converterRegistry)
		{
			if (editorNodeModels == null || converterRegistry == null)
			{
				return Array.Empty<Type>();
			}

			HashSet<Type> unsupported = new HashSet<Type>();
			foreach (object model in editorNodeModels)
			{
				if (model == null)
				{
					continue;
				}

				if (converterRegistry.TryConvert(model, out _))
				{
					continue;
				}

				unsupported.Add(model.GetType());
			}

			return unsupported.ToList();
		}

		private static IEnumerable<Type> EnumerateTypesWithAttribute<TAttribute>(IEnumerable<Assembly> assemblies) where TAttribute : Attribute
		{
			if (assemblies == null)
			{
				yield break;
			}

			HashSet<Type> unique = new HashSet<Type>();

			foreach (Assembly assembly in assemblies)
			{
				if (assembly == null)
				{
					continue;
				}

				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException exception)
				{
					types = exception.Types;
				}

				if (types == null)
				{
					continue;
				}

				for (int i = 0; i < types.Length; i++)
				{
					Type type = types[i];
					if (type == null || unique.Contains(type))
					{
						continue;
					}

					if (type.GetCustomAttribute<TAttribute>() != null)
					{
						unique.Add(type);
					}
				}
			}

			// deterministic order
			List<Type> ordered = unique.OrderBy(t => t.FullName).ToList();
			for (int i = 0; i < ordered.Count; i++)
			{
				yield return ordered[i];
			}
		}

		private static bool TryCreateInstance<T>(Type type, out T instance) where T : class
		{
			instance = null;
			if (type == null)
			{
				return false;
			}

			try
			{
				object obj = Activator.CreateInstance(type);
				instance = obj as T;
				return instance != null;
			}
			catch
			{
				return false;
			}
		}
	}
}
