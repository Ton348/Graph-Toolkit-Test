using System;

namespace Game1.Graph.Runtime
{
	public interface IGameGraphNodeValidator
	{
		Type NodeType { get; }
		bool Validate(GameGraphNode node, GameGraphValidationResult result);
	}
}
