using System;
using System.Collections.Generic;

public sealed class GameGraphNodeValidatorRegistry
{
	private readonly Dictionary<Type, IGameGraphNodeValidator> m_validatorsByNodeType = new Dictionary<Type, IGameGraphNodeValidator>();
	private readonly Dictionary<Type, IGameGraphNodeValidator> m_resolvedValidatorCache = new Dictionary<Type, IGameGraphNodeValidator>();

	public void Register(IGameGraphNodeValidator validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException(nameof(validator));
		}

		if (validator.NodeType == null)
		{
			throw new InvalidOperationException($"Validator '{validator.GetType().Name}' has null NodeType.");
		}

		if (m_validatorsByNodeType.TryGetValue(validator.NodeType, out IGameGraphNodeValidator existingValidator))
		{
			// replace existing
			m_validatorsByNodeType[validator.NodeType] = validator;
			m_resolvedValidatorCache.Clear();
			return;
		}

		m_validatorsByNodeType.Add(validator.NodeType, validator);
		m_resolvedValidatorCache.Clear();
	}

	public void Register<TValidator>() where TValidator : IGameGraphNodeValidator, new()
	{
		Register(new TValidator());
	}

	public bool TryGetValidator(Type nodeType, out IGameGraphNodeValidator validator)
	{
		if (nodeType == null)
		{
			validator = null;
			return false;
		}

		if (m_resolvedValidatorCache.TryGetValue(nodeType, out validator))
		{
			return validator != null;
		}

		Type currentType = nodeType;
		while (currentType != null && currentType != typeof(object))
		{
			if (m_validatorsByNodeType.TryGetValue(currentType, out validator))
			{
				m_resolvedValidatorCache[nodeType] = validator;
				return true;
			}

			currentType = currentType.BaseType;
		}

		m_resolvedValidatorCache[nodeType] = null;
		validator = null;
		return false;
	}

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (node == null)
		{
			if (result != null)
			{
				result.AddError(null, nameof(node), "Node is null.");
			}
			return false;
		}

		if (!TryGetValidator(node.GetType(), out IGameGraphNodeValidator validator))
		{
			return true;
		}

		return validator.Validate(node, result);
	}

	public GameGraphValidationResult ValidateAll(IEnumerable<GameGraphNode> nodes)
	{
		GameGraphValidationResult result = new GameGraphValidationResult();
		if (nodes == null)
		{
			result.AddWarning(null, nameof(nodes), "Nodes collection is null.");
			return result;
		}

		foreach (GameGraphNode node in nodes)
		{
			Validate(node, result);
		}

		return result;
	}
}
