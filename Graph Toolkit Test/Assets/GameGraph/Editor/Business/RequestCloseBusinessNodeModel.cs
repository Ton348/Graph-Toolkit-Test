using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestCloseBusinessNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";

	protected override string defaultTitle => "Закрыть бизнес";
	protected override string defaultDescription => "Запрашивает закрытие бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
	}
}