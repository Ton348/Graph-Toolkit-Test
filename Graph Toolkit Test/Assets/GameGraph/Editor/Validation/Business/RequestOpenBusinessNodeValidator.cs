using System;
using Game1.Graph.Runtime;

[GameGraphNodeValidator]
public sealed class RequestOpenBusinessNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(RequestOpenBusinessNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestOpenBusinessNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode, nameof(typedNode.successNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode, nameof(typedNode.failNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId), result);
		return valid;
	}
}
