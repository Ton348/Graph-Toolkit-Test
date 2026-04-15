using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestOpenBusinessNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";

	protected override string DefaultTitle => "Открыть бизнес";
	protected override string DefaultDescription => "Запрашивает открытие бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
	}
}
