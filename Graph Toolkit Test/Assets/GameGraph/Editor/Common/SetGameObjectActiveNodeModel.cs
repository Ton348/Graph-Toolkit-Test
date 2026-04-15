using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;
using GraphCore.Runtime;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class SetGameObjectActiveNodeModel : GameGraphEditorNode
{
	public const string SiteIdOption = "SiteId";
	public const string VisualIdOption = "VisualId";
	public const string IsActiveOption = "IsActive";

	protected override string DefaultTitle => "Активировать объект";
	protected override string DefaultDescription => "Изменяет активность визуального объекта.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(SiteIdOption).WithDisplayName("SiteId");
		context.AddOption<string>(VisualIdOption).WithDisplayName("VisualId");
		context.AddOption<bool>(IsActiveOption).WithDisplayName("IsActive");
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
