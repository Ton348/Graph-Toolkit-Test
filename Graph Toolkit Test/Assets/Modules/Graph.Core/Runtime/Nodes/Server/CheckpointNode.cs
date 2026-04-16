using System;

namespace GraphCore.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class CheckpointNode : BaseGraphNode
	{
		public string checkpointId;
		public CheckpointAction action;
		public string successNodeId;
		public string failNodeId;

		public CheckpointNode()
		{
			Title = "Checkpoint";
			Description = "Saves or clears checkpoint state";
		}
	}
}
