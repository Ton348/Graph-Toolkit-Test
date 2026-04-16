using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestInstallBusinessModuleNode : GameGraphSuccessFailNode
	{
		public string lotId;
		public string moduleId;
	}
}