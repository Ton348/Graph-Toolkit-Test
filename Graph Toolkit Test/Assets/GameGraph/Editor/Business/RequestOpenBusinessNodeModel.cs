using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using GraphCore.Runtime;
[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestOpenBusinessNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";

	protected override string defaultTitle => "Открыть бизнес";
	protected override string defaultDescription => "Запрашивает открытие бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
	}
}