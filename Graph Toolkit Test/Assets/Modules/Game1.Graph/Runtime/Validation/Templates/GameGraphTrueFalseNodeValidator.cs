using System;

[GameGraphNodeValidator]
public sealed class GameGraphTrueFalseNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(GameGraphTrueFalseNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (node is not GameGraphTrueFalseNode typedNode)
		{
			result?.AddError(node, nameof(node), "Invalid node type for GameGraphTrueFalseNodeValidator.");
			return false;
		}

		bool valid = true;
		if (string.IsNullOrWhiteSpace(typedNode.trueNodeId))
		{
			result?.AddError(typedNode, nameof(typedNode.trueNodeId), "True node id is required.");
			valid = false;
		}

		if (string.IsNullOrWhiteSpace(typedNode.falseNodeId))
		{
			result?.AddError(typedNode, nameof(typedNode.falseNodeId), "False node id is required.");
			valid = false;
		}

		return valid;
	}
}
