using System;
using Game1.Graph.Runtime;

[GameGraphNodeValidator]
public sealed class RequestBuyBuildingNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(RequestBuyBuildingNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestBuyBuildingNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode, nameof(typedNode.successNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode, nameof(typedNode.failNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.buildingId, typedNode, nameof(typedNode.buildingId), result);
		if (typedNode.questAction != QuestActionType.None)
		{
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.questId, typedNode, nameof(typedNode.questId), result);
		}
		return valid;
	}
}
