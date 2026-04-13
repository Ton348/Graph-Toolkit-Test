using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessExistsNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LOT_ID_OPTION = "LotId";

	protected override string DefaultTitle => "Проверка существования бизнеса";
	protected override string DefaultDescription => "Проверяет, есть ли бизнес на участке.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
	}
}
