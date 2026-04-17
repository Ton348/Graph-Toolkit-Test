using System;
using Game1.Graph.Runtime;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using GameGraph.Editor.Validation.Infrastructure;
using GameGraph.Runtime.Common;

namespace GameGraph.Editor.Validation.Common
{
	[GameGraphNodeValidator]
	public sealed class SetGameObjectActiveNodeDataValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(SetGameObjectActiveNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out SetGameObjectActiveNode typedNode))
			{
				return false;
			}

			var valid = true;
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.nextNodeId, typedNode,
				nameof(typedNode.nextNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.siteId, typedNode,
				nameof(typedNode.siteId), result);
			if (typedNode.isActive)
			{
				bool hasVisual = !string.IsNullOrWhiteSpace(typedNode.visualId) || typedNode.targetObject != null;
				if (!hasVisual)
				{
					result?.AddError(typedNode, nameof(typedNode.visualId),
						"visualId or targetObject is required when isActive is true.");
					valid = false;
				}
			}

			return valid;
		}
	}
}