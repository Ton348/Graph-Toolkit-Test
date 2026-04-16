using System;
using Game1.Graph.Runtime;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using GameGraph.Editor.Validation.Infrastructure;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Validation.Business
{
	[GameGraphNodeValidator]
	public sealed class RequestSetBusinessMarkupNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(RequestSetBusinessMarkupNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestSetBusinessMarkupNode typedNode))
			{
				return false;
			}

			var valid = true;
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode,
				nameof(typedNode.successNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode,
				nameof(typedNode.failNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId),
				result);
			if (typedNode.markupPercent < 0 || typedNode.markupPercent > 100)
			{
				result?.AddError(typedNode, nameof(typedNode.markupPercent), "markupPercent must be in [0..100].");
				valid = false;
			}

			return valid;
		}
	}
}