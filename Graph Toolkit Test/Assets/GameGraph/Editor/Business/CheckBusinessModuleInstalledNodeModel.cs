using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessModuleInstalledNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LotIdOption = "LotId";
	public const string ModuleIdOption = "ModuleId";

	protected override string defaultTitle => "Проверка установки модуля";
	protected override string defaultDescription => "Проверяет, установлен ли модуль в бизнесе.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		context.AddOption<string>(ModuleIdOption).WithDisplayName("ModuleId");
	}
}
