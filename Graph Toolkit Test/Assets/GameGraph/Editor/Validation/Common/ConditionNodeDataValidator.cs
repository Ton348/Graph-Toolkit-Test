using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeValidator]
public sealed class ConditionNodeDataValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(ConditionNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out ConditionNode typedNode))
		{
			return false;
		}

		bool valid = true;
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.trueNodeId, typedNode, nameof(typedNode.trueNodeId), result);
		valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.falseNodeId, typedNode, nameof(typedNode.falseNodeId), result);

		valid &= typedNode.conditionType switch
		{
			ConditionType.BuildingOwned => GameGraphValidationHelpers.ValidateRequiredString(typedNode.buildingId, typedNode, nameof(typedNode.buildingId), result),
			ConditionType.QuestActive => GameGraphValidationHelpers.ValidateRequiredString(typedNode.questId, typedNode, nameof(typedNode.questId), result),
			ConditionType.QuestCompleted => GameGraphValidationHelpers.ValidateRequiredString(typedNode.questId, typedNode, nameof(typedNode.questId), result),
			_ => true
		};

		return valid;
	}
}
