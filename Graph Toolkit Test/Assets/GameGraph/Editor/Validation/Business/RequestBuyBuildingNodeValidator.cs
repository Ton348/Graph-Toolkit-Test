using System;
using Game1.Graph.Runtime;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using GameGraph.Editor.Validation.Infrastructure;
using GameGraph.Runtime.Business;
using GameGraph.Runtime.Quest;

namespace GameGraph.Editor.Validation.Business
{
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

			var valid = true;
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode,
				nameof(typedNode.successNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode,
				nameof(typedNode.failNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.buildingId, typedNode,
				nameof(typedNode.buildingId), result);
			if (typedNode.questAction != QuestActionType.None)
			{
				valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.questId, typedNode,
					nameof(typedNode.questId), result);
			}

			return valid;
		}
	}
}