using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class SetGameObjectActiveNodeModel : GameGraphEditorNode
{
	public const string SiteIdOption = "SiteId";
	public const string VisualIdOption = "VisualId";
	public const string IsActiveOption = "IsActive";

	protected override string defaultTitle => "Активировать объект";
	protected override string defaultDescription => "Изменяет активность визуального объекта.";

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