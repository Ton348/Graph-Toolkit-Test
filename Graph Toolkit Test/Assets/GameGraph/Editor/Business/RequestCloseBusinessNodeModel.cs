using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestCloseBusinessNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";

	protected override string DefaultTitle => "Закрыть бизнес";
	protected override string DefaultDescription => "Запрашивает закрытие бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
	}
}
