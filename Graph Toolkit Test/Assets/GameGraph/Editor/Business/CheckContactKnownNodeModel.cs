using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckContactKnownNodeModel : GameGraphTrueFalseNodeModel
{
	public const string CONTACT_ID_OPTION = "ContactId";

	protected override string DefaultTitle => "Проверка знакомства с контактом";
	protected override string DefaultDescription => "Проверяет, знаком ли игрок с контактом.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(CONTACT_ID_OPTION).WithDisplayName("ContactId");
	}
}
