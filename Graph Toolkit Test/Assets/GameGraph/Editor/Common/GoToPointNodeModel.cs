using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class GoToPointNodeModel : GameGraphEditorNode
{
	public const string MarkerIdOption = "MarkerId";
	public const string ArrivalDistanceOption = "ArrivalDistance";

	protected override string defaultTitle => "Перейти к точке";
	protected override string defaultDescription => "Задает целевую точку для перемещения.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(MarkerIdOption).WithDisplayName("MarkerId");
		context.AddOption<float>(ArrivalDistanceOption).WithDisplayName("ArrivalDistance").WithDefaultValue(2f);
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}