using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestSetBusinessMarkupNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string MARKUP_PERCENT_OPTION = "MarkupPercent";

	protected override string DefaultTitle => "Изменить наценку бизнеса";
	protected override string DefaultDescription => "Запрашивает изменение наценки бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<int>(MARKUP_PERCENT_OPTION).WithDisplayName("MarkupPercent");
	}
}
