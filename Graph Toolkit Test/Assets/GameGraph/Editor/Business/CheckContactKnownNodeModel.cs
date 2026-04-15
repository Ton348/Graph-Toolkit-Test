using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckContactKnownNodeModel : GameGraphTrueFalseNodeModel
{
	public const string ContactIdOption = "ContactId";

	protected override string DefaultTitle => "Проверка знакомства с контактом";
	protected override string DefaultDescription => "Проверяет, знаком ли игрок с контактом.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(ContactIdOption).WithDisplayName("ContactId");
	}
}
