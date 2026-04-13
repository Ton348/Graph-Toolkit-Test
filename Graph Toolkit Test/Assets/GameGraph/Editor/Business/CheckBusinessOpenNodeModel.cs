using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessOpenNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LOT_ID_OPTION = "LotId";

	protected override string DefaultTitle => "Проверка открыт ли бизнес";
	protected override string DefaultDescription => "Проверяет, открыт ли бизнес.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
	}
}
