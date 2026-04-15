using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestHireBusinessWorkerNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string ROLE_ID_OPTION = "RoleId";
	public const string CONTACT_ID_OPTION = "ContactId";

	protected override string DefaultTitle => "Нанять сотрудника";
	protected override string DefaultDescription => "Запрашивает найм сотрудника для бизнеса.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<string>(ROLE_ID_OPTION).WithDisplayName("RoleId");
		context.AddOption<string>(CONTACT_ID_OPTION).WithDisplayName("ContactId");
	}
}
