using System;

[Serializable]
public abstract class GameGraphSuccessFailNode : GameGraphNode
{
	public string successNodeId;
	public string failNodeId;
}
