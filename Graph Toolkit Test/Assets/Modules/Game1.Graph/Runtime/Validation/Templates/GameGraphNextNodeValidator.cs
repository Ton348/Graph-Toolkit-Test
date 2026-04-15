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
	public sealed class GameGraphNextNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(GameGraphNextNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (node is not GameGraphNextNode typedNode)
			{
				result?.AddError(node, nameof(node), "Invalid node type for GameGraphNextNodeValidator.");
				return false;
			}

			if (!string.IsNullOrWhiteSpace(typedNode.nextNodeId))
			{
				return true;
			}

			result?.AddError(typedNode, nameof(typedNode.nextNodeId), "Next node id is required.");
			return false;
		}
	}
}
