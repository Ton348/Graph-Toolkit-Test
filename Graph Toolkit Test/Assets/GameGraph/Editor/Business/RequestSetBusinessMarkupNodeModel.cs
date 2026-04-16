using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestSetBusinessMarkupNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";
	public const string MarkupPercentOption = "MarkupPercent";

	protected override string defaultTitle => "Изменить наценку бизнеса";
	protected override string defaultDescription => "Запрашивает изменение наценки бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		context.AddOption<int>(MarkupPercentOption).WithDisplayName("MarkupPercent");
	}
}