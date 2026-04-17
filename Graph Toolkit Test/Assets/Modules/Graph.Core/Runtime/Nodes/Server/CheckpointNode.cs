using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class CheckpointNode : CoreGraphSuccessFailNode
	{
		public string checkpointId;
		public CheckpointAction action;

		public CheckpointNode()
		{
			Title = "Checkpoint";
			Description = "Saves or clears checkpoint state";
		}
	}
}