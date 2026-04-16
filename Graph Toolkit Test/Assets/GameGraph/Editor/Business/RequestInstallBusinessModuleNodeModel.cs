using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using GraphCore.Runtime;
[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestInstallBusinessModuleNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";
	public const string ModuleIdOption = "ModuleId";

	protected override string defaultTitle => "Установить модуль бизнеса";
	protected override string defaultDescription => "Запрашивает установку модуля для бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		context.AddOption<string>(ModuleIdOption).WithDisplayName("ModuleId");
	}
}