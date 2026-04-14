using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class SetGameObjectActiveNodeModel : GameGraphEditorNode
{
	public const string SITE_ID_OPTION = "SiteId";
	public const string VISUAL_ID_OPTION = "VisualId";
	public const string IS_ACTIVE_OPTION = "IsActive";

	protected override string DefaultTitle => "Активировать объект";
	protected override string DefaultDescription => "Изменяет активность визуального объекта.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(SITE_ID_OPTION).WithDisplayName("SiteId");
		context.AddOption<string>(VISUAL_ID_OPTION).WithDisplayName("VisualId");
		context.AddOption<bool>(IS_ACTIVE_OPTION).WithDisplayName("IsActive");
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
