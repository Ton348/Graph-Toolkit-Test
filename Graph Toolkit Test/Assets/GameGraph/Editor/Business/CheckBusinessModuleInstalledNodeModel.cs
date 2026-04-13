using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessModuleInstalledNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string MODULE_ID_OPTION = "ModuleId";

	protected override string DefaultTitle => "Проверка установки модуля";
	protected override string DefaultDescription => "Проверяет, установлен ли модуль в бизнесе.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<string>(MODULE_ID_OPTION).WithDisplayName("ModuleId");
	}
}
