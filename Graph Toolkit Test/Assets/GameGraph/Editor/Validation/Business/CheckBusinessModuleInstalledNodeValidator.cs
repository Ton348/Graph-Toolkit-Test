using System;
using Game1.Graph.Runtime;

[GameGraphNodeValidator]
public sealed class CheckBusinessModuleInstalledNodeValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(CheckBusinessModuleInstalledNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out CheckBusinessModuleInstalledNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.trueNodeId, typedNode, nameof(typedNode.trueNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.falseNodeId, typedNode, nameof(typedNode.falseNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId), result);
		valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.moduleId, typedNode, nameof(typedNode.moduleId), result);
		return valid;
	}
}
