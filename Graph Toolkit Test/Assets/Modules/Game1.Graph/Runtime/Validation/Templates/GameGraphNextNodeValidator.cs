using System;

namespace Game1.Graph.Runtime
{
	[GameGraphNodeValidator]
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
