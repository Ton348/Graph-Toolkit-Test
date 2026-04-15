using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeValidator]
public sealed class RequestInstallBusinessModuleNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(RequestInstallBusinessModuleNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestInstallBusinessModuleNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode, nameof(typedNode.successNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode, nameof(typedNode.failNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.moduleId, typedNode, nameof(typedNode.moduleId), result);
		return valid;
	}
}
