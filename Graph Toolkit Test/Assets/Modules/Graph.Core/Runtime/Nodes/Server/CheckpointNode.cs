using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Server
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