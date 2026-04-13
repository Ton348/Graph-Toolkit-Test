using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestSetBusinessOpenNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string OPEN_OPTION = "Open";

	protected override string DefaultTitle => "Установить статус открытия";
	protected override string DefaultDescription => "Устанавливает желаемый статус открытия бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<bool>(OPEN_OPTION).WithDisplayName("Open");
	}
}
