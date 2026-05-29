using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class CheckThreatScoreNode : BaseGraphNode
	{
		public DangerCompareType compareType = DangerCompareType.Greater;
		public int valueA = 10;
		public int valueB = 20;
		public string trueNodeId;
		public string falseNodeId;

		public CheckThreatScoreNode()
		{
			Title = "Проверка угрозы";
			Description = "Сравнивает текущий threatScore с заданными значениями.";
		}
	}
}
