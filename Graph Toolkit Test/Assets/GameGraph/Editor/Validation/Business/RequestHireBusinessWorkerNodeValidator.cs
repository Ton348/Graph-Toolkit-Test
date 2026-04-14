using System;

[GameGraphNodeValidator]
public sealed class RequestHireBusinessWorkerNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(RequestHireBusinessWorkerNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestHireBusinessWorkerNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode, nameof(typedNode.successNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode, nameof(typedNode.failNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.roleId, typedNode, nameof(typedNode.roleId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.contactId, typedNode, nameof(typedNode.contactId), result);
		return valid;
	}
}
