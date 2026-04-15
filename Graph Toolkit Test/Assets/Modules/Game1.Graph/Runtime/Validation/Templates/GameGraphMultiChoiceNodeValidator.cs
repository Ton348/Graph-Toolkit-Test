using System;
using GraphCore.Runtime;

using Game1.Graph.Runtime;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Templates;
using Game1.Graph.Runtime.Validation;
namespace Game1.Graph.Runtime.Validation.Templates
{
	[Game1.Graph.Runtime.Infrastructure.AutoRegistration.GameGraphNodeValidator]
	public sealed class GameGraphMultiChoiceNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(GameGraphMultiChoiceNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (node is not GameGraphMultiChoiceNode typedNode)
			{
				result?.AddError(node, nameof(node), "Invalid node type for GameGraphMultiChoiceNodeValidator.");
				return false;
			}

			if (typedNode.options == null || typedNode.options.Count == 0)
			{
				result?.AddError(typedNode, nameof(typedNode.options), "At least one choice option is required.");
				return false;
			}

			bool valid = true;
			for (int i = 0; i < typedNode.options.Count; i++)
			{
				GameGraphChoiceBranch option = typedNode.options[i];
				if (option == null)
				{
					result?.AddError(typedNode, nameof(typedNode.options), $"Option at index {i} is null.");
					valid = false;
					continue;
				}

				if (string.IsNullOrWhiteSpace(option.nextNodeId))
				{
					result?.AddError(typedNode, nameof(option.nextNodeId), $"Option {i} next node id is required.");
					valid = false;
				}
			}

			return valid;
		}
	}
}
