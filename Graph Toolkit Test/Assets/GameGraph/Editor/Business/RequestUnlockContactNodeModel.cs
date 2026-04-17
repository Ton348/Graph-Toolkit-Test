using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestUnlockContactNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string ContactIdOption = "ContactId";

		protected override string defaultTitle => "Разблокировать контакт";
		protected override string defaultDescription => "Запрашивает разблокировку контакта.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(ContactIdOption).WithDisplayName("ContactId");
		}
	}
}