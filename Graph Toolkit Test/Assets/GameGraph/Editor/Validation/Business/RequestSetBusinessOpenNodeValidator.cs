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
	public sealed class RequestSetBusinessOpenNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(RequestSetBusinessOpenNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestSetBusinessOpenNode typedNode))
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
			return valid;
		}
	}
}