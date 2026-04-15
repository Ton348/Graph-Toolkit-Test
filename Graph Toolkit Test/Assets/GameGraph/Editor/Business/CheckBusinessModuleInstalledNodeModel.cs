using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessModuleInstalledNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LotIdOption = "LotId";
	public const string ModuleIdOption = "ModuleId";

	protected override string DefaultTitle => "Проверка установки модуля";
	protected override string DefaultDescription => "Проверяет, установлен ли модуль в бизнесе.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		context.AddOption<string>(ModuleIdOption).WithDisplayName("ModuleId");
	}
}
