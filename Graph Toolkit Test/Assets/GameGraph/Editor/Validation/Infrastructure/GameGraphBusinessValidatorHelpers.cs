using System;
using Game1.Graph.Runtime;

public static class GameGraphBusinessValidatorHelpers
{
	public static bool ValidateType<TNode>(GameGraphNode node, GameGraphValidationResult result, out TNode typedNode) where TNode : GameGraphNode
	{
		typedNode = node as TNode;
		if (typedNode != null)
		{
			return true;
		}

		result?.AddError(node, nameof(node), $"Invalid node type for validator '{typeof(TNode).Name}'.");
		return false;
	}
}
