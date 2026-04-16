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
	public sealed class CheckBusinessModuleInstalledNodeValidator : IGameGraphNodeValidator
	{
		public Type NodeType => typeof(CheckBusinessModuleInstalledNode);

		public bool Validate(GameGraphNode node, GameGraphValidationResult result)
		{
			if (!GameGraphBusinessValidatorHelpers.ValidateType(node, result,
				    out CheckBusinessModuleInstalledNode typedNode))
			{
				return false;
			}

			var valid = true;
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.trueNodeId, typedNode,
				nameof(typedNode.trueNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateNodeId(typedNode.falseNodeId, typedNode,
				nameof(typedNode.falseNodeId), result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.lotId, typedNode, nameof(typedNode.lotId),
				result);
			valid &= GameGraphValidationHelpers.ValidateRequiredString(typedNode.moduleId, typedNode,
				nameof(typedNode.moduleId), result);
			return valid;
		}
	}
}