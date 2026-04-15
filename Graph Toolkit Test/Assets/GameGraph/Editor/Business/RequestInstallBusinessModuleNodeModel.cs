using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestInstallBusinessModuleNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string MODULE_ID_OPTION = "ModuleId";

	protected override string DefaultTitle => "Установить модуль бизнеса";
	protected override string DefaultDescription => "Запрашивает установку модуля для бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<string>(MODULE_ID_OPTION).WithDisplayName("ModuleId");
	}
}
