
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class GameGraphRuntimeRegistryFactory
{
    public static GraphNodeExecutorRegistry Create()
    {
        GameGraphComposition composition = GameGraphComposition.CreateDefault();

        HashSet<Assembly> assemblies = new HashSet<Assembly>();
        Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < loadedAssemblies.Length; i++)
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

            for (int i = 0; i < types.Length; i++)
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

                try
                {
                    IGraphNodeExecutor executor = (IGraphNodeExecutor)Activator.CreateInstance(type);
                    composition.ExecutorRegistry.Register(executor);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[GameGraphRuntimeRegistryFactory] Failed to register executor '{type.FullName}': {exception.Message}");
                }
            }
        }

        return composition.CreateRuntimeExecutorRegistry();
    }
}
