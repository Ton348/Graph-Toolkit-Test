using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class CheckBusinessExistsNodeModel : GameGraphTrueFalseNodeModel
{
	public const string LotIdOption = "LotId";

	protected override string defaultTitle => "Проверка существования бизнеса";
	protected override string defaultDescription => "Проверяет, есть ли бизнес на участке.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
	}
}
