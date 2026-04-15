using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessExistsNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LotIdOption = "LotId";

	protected override string DefaultTitle => "Проверка существования бизнеса";
	protected override string DefaultDescription => "Проверяет, есть ли бизнес на участке.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
	}
}
