using System;

namespace GraphCore.Runtime.Nodes.Utility
{
	[Serializable]
	public sealed class LogNode : BaseGraphNode
	{
		public string message;

		public LogNode()
		{
			Title = "Log";
			Description = "Writes a debug message";
		}
	}
}
