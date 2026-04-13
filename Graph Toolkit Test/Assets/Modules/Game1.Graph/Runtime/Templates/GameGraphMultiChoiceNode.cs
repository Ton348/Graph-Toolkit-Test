using System;
using System.Collections.Generic;

[Serializable]
public abstract class GameGraphMultiChoiceNode : GameGraphNode
{
	public List<GameGraphChoiceBranch> options = new List<GameGraphChoiceBranch>();
}
