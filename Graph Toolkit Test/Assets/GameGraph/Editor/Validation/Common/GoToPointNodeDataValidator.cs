using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Validation;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeValidator]
public sealed class GoToPointNodeDataValidator : IGameGraphNodeValidator
{
	public Type NodeType => typeof(GoToPointNode);

	public bool Validate(GameGraphNode node, GameGraphValidationResult result)
	{
		if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result, out GoToPointNode typedNode))
		{
			return false;
		}

		bool valid = GameGraphValidationHelpers.ValidateNodeId(typedNode.nextNodeId, typedNode, nameof(typedNode.nextNodeId), result);

		bool hasMarker = !string.IsNullOrWhiteSpace(typedNode.markerId);
		bool hasTransform = typedNode.targetTransform != null;
		if (hasMarker || hasTransform)
		{
			return valid;
		}

		result?.AddError(typedNode, nameof(typedNode.markerId), "Either markerId or targetTransform must be set.");
		return false;
	}
}
