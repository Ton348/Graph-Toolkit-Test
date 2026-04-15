using System;
using Game1.Graph.Runtime;

[GameGraphNodeValidator]
public sealed class RequestTradeOfferNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(RequestTradeOfferNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestTradeOfferNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode, nameof(typedNode.successNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode, nameof(typedNode.failNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.buildingId, typedNode, nameof(typedNode.buildingId), result);
		return valid;
	}
}
