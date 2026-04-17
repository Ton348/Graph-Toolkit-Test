using System;
using Game1.Graph.Runtime.Infrastructure.Validation;

namespace Game1.Graph.Runtime.Validation
{
	public interface IGameGraphNodeValidator
	{
		Type NodeType { get; }
		bool Validate(GameGraphNode node, GameGraphValidationResult result);
	}
}