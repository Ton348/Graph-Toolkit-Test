using System;

public interface IGameGraphNodeValidator
{
	Type NodeType { get; }
	bool Validate(GameGraphNode node, GameGraphValidationResult result);
}
