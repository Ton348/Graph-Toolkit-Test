using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestHireBusinessWorkerNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";
	public const string RoleIdOption = "RoleId";
	public const string ContactIdOption = "ContactId";

	protected override string defaultTitle => "Нанять сотрудника";
	protected override string defaultDescription => "Запрашивает найм сотрудника для бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		context.AddOption<string>(RoleIdOption).WithDisplayName("RoleId");
		context.AddOption<string>(ContactIdOption).WithDisplayName("ContactId");
	}
}