using System;
using System.Collections.Generic;
using System.Reflection;
using Game1.Graph.Editor.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using Graph.Core.Runtime;

namespace Game1.Graph.Editor.Infrastructure.Validation
{
	public sealed class GameGraphModuleBuilder
	{
		private readonly List<Assembly> m_autoRegistrationAssemblies = new();
		private readonly List<IGameGraphNodeConverter> m_converters = new();
		private readonly List<IGraphNodeExecutor> m_executors = new();
		private readonly List<IGameGraphNodeValidator> m_validators = new();

		public GameGraphModuleBuilder WithAutoRegistration(params Assembly[] assemblies)
		{
			if (assemblies == null)
			{
				return this;
			}

			for (var i = 0; i < assemblies.Length; i++)
			{
				Assembly assembly = assemblies[i];
				if (assembly == null || m_autoRegistrationAssemblies.Contains(assembly))
				{
					continue;
				}

				m_autoRegistrationAssemblies.Add(assembly);
			}

			return this;
		}

		public GameGraphModuleBuilder WithAutoRegistration(params Type[] markerTypes)
		{
			IReadOnlyList<Assembly> assemblies = GameGraphAutoRegistration.BuildAssemblySet(markerTypes);
			for (var i = 0; i < assemblies.Count; i++)
			{
				Assembly assembly = assemblies[i];
				if (assembly == null || m_autoRegistrationAssemblies.Contains(assembly))
				{
					continue;
				}

				m_autoRegistrationAssemblies.Add(assembly);
			}

			return this;
		}

		public GameGraphModuleBuilder WithExecutor(IGraphNodeExecutor executor)
		{
			if (executor == null)
			{
				return this;
			}

			m_executors.Add(executor);
			return this;
		}

		public GameGraphModuleBuilder WithExecutor<TExecutor>() where TExecutor : IGraphNodeExecutor, new()
		{
			m_executors.Add(new TExecutor());
			return this;
		}

		public GameGraphModuleBuilder WithExecutors(IEnumerable<IGraphNodeExecutor> executors)
		{
			if (executors == null)
			{
				return this;
			}

			foreach (IGraphNodeExecutor executor in executors)
			{
				if (executor == null)
				{
					continue;
				}

				m_executors.Add(executor);
			}

			return this;
		}

		public GameGraphModuleBuilder WithConverters(IEnumerable<IGameGraphNodeConverter> converters)
		{
			if (converters == null)
			{
				return this;
			}

			foreach (IGameGraphNodeConverter converter in converters)
			{
				if (converter == null)
				{
					continue;
				}

				m_converters.Add(converter);
			}

			return this;
		}

		public GameGraphModuleBuilder WithValidation(IEnumerable<IGameGraphNodeValidator> validators)
		{
			if (validators == null)
			{
				return this;
			}

			foreach (IGameGraphNodeValidator validator in validators)
			{
				if (validator == null)
				{
					continue;
				}

				m_validators.Add(validator);
			}

			return this;
		}

		public GameGraphModule Build()
		{
			var runtimeComposition = GameGraphComposition.CreateDefault();
			var editorComposition = GameGraphEditorComposition.CreateDefault();
			var validationComposition = GameGraphValidationComposition.CreateDefault();

			for (var i = 0; i < m_executors.Count; i++)
			{
				runtimeComposition.ExecutorRegistry.Register(m_executors[i]);
			}

			for (var i = 0; i < m_converters.Count; i++)
			{
				editorComposition.ConverterRegistry.Register(m_converters[i]);
			}

			for (var i = 0; i < m_validators.Count; i++)
			{
				validationComposition.RegisterValidator(m_validators[i]);
			}

			if (m_autoRegistrationAssemblies.Count > 0)
			{
				GameGraphAutoRegistration.RegisterExecutors(runtimeComposition.ExecutorRegistry,
					m_autoRegistrationAssemblies);
				GameGraphAutoRegistration.RegisterConverters(editorComposition.ConverterRegistry,
					m_autoRegistrationAssemblies);
				GameGraphAutoRegistration.RegisterValidators(validationComposition.ValidatorRegistry,
					m_autoRegistrationAssemblies);
			}

			return new GameGraphModule(runtimeComposition, editorComposition, validationComposition);
		}

		public GameGraphModule BuildEditor()
		{
			return Build();
		}
	}
}