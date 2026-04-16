using System;
using System.Collections.Generic;
using System.Reflection;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GraphCore.Runtime;
using UnityEngine;

namespace Game1.Graph.Runtime.Infrastructure
{
	public static class GameGraphRuntimeRegistryFactory
	{
		private static readonly object s_cacheLock = new();
		private static Type[] s_cachedExecutorTypes;

		public static GraphNodeExecutorRegistry Create()
		{
			var composition = GameGraphComposition.CreateDefault();
			Type[] executorTypes = GetCachedExecutorTypes();

			for (var i = 0; i < executorTypes.Length; i++)
			{
				Type type = executorTypes[i];
				try
				{
					var executor = (IGraphNodeExecutor)Activator.CreateInstance(type);
					composition.ExecutorRegistry.Register(executor);
				}
				catch (Exception exception)
				{
					Debug.LogError(
						$"[GameGraphRuntimeRegistryFactory] Failed to register executor '{type.FullName}': {exception.Message}");
				}
			}

			return composition.CreateRuntimeExecutorRegistry();
		}

		private static Type[] GetCachedExecutorTypes()
		{
			lock (s_cacheLock)
			{
				if (s_cachedExecutorTypes != null)
				{
					return s_cachedExecutorTypes;
				}

				var executorTypes = new List<Type>();
				var assemblies = new HashSet<Assembly>();
				Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (var i = 0; i < loadedAssemblies.Length; i++)
				{
					Assembly assembly = loadedAssemblies[i];
					if (assembly == null || assembly.IsDynamic)
					{
						continue;
					}

					assemblies.Add(assembly);
				}

				foreach (Assembly assembly in assemblies)
				{
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

					for (var i = 0; i < types.Length; i++)
					{
						Type type = types[i];
						if (type == null || type.IsAbstract || type.IsGenericTypeDefinition)
						{
							continue;
						}

						if (!typeof(IGraphNodeExecutor).IsAssignableFrom(type))
						{
							continue;
						}

						if (type.GetCustomAttribute<GameGraphNodeExecutorAttribute>() == null)
						{
							continue;
						}

						if (type.GetConstructor(Type.EmptyTypes) == null)
						{
							continue;
						}

						executorTypes.Add(type);
					}
				}

				s_cachedExecutorTypes = executorTypes.ToArray();
				return s_cachedExecutorTypes;
			}
		}
	}
}