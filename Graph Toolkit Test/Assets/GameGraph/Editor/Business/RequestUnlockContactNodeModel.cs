using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestUnlockContactNodeModel : GameGraphSuccessFailNodeModel
{
	public const string CONTACT_ID_OPTION = "ContactId";

	protected override string DefaultTitle => "Разблокировать контакт";
	protected override string DefaultDescription => "Запрашивает разблокировку контакта.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(CONTACT_ID_OPTION).WithDisplayName("ContactId");
	}
}
