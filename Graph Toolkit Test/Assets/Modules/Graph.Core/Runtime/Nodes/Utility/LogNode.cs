using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Utility
{
	[Serializable]
	public sealed class LogNode : CoreGraphNextNode
	{
		public string message;

		public LogNode()
		{
			Title = "Log";
			Description = "Writes a debug message";
		}
	}
}