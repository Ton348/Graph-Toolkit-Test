using System;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Infrastructure.Validation;
using Game1.Graph.Runtime.Templates;

namespace Game1.Graph.Runtime.Validation.Templates
{
	[GameGraphNodeValidator]
	public sealed class GameGraphSuccessFailNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(GameGraphSuccessFailNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (node is not GameGraphSuccessFailNode typedNode)
			{
				result?.AddError(node, nameof(node), "Invalid node type for GameGraphSuccessFailNodeValidator.");
				return false;
			}

			var valid = true;
			if (string.IsNullOrWhiteSpace(typedNode.successNodeId))
			{
				result?.AddError(typedNode, nameof(typedNode.successNodeId), "Success node id is required.");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(typedNode.failNodeId))
			{
				result?.AddError(typedNode, nameof(typedNode.failNodeId), "Fail node id is required.");
				valid = false;
			}

			return valid;
		}
	}
}