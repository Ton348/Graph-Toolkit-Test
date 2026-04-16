using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestSetBusinessOpenNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string LotIdOption = "LotId";
		public const string OpenOption = "Open";

		protected override string defaultTitle => "Установить статус открытия";
		protected override string defaultDescription => "Устанавливает желаемый статус открытия бизнеса.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
			context.AddOption<bool>(OpenOption).WithDisplayName("Open");
		}
	}
}