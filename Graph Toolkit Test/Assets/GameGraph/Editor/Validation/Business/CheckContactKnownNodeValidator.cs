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
	public sealed class CheckContactKnownNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(CheckContactKnownNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out CheckContactKnownNode typedNode))
			{
				return false;
			}

			var valid = true;
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.trueNodeId, typedNode,
				nameof(typedNode.trueNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.falseNodeId, typedNode,
				nameof(typedNode.falseNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.contactId, typedNode,
				nameof(typedNode.contactId), result);
			return valid;
		}
	}
}