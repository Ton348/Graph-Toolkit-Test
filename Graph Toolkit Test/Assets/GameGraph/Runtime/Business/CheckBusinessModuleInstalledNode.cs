using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class CheckBusinessModuleInstalledNode : GameGraphTrueFalseNode
	{
		public string lotId;
		public string moduleId;
	}
}