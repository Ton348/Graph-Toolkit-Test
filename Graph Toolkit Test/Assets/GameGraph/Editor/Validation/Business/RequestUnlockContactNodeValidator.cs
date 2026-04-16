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
	public sealed class RequestUnlockContactNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(RequestUnlockContactNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out RequestUnlockContactNode typedNode))
			{
				return false;
			}

			var valid = true;
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.successNodeId, typedNode,
				nameof(typedNode.successNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.failNodeId, typedNode,
				nameof(typedNode.failNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.contactId, typedNode,
				nameof(typedNode.contactId), result);
			return valid;
		}
	}
}