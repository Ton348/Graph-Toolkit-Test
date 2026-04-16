using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using GraphCore.Runtime;
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