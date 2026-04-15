using System;
using Game1.Graph.Runtime;

[GameGraphNodeValidator]
public sealed class CheckBusinessOpenNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(CheckBusinessOpenNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out CheckBusinessOpenNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.trueNodeId, typedNode, nameof(typedNode.trueNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.falseNodeId, typedNode, nameof(typedNode.falseNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId), result);
		return valid;
	}
}
