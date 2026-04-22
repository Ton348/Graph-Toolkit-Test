using System;

namespace Graph.Core.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class FinishNode : BaseGraphNode
	{
		public FinishNode()
		{
			Title = "Завершение графа";
			Description = "Завершает выполнение сценария";
		}
	}
}
