using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Utility
{
	[Serializable]
	public sealed class LogNode : CoreGraphNextNode
	{
		public string message;

		public LogNode()
		{
			Title = "Лог";
			Description = "Выводит сообщение в консоль";
		}
	}
}
