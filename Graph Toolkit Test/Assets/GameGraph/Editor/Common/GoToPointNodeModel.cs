using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class GoToPointNodeModel : GameGraphEditorNode
{
	public const string MARKER_ID_OPTION = "MarkerId";
	public const string ARRIVAL_DISTANCE_OPTION = "ArrivalDistance";

	protected override string DefaultTitle => "Перейти к точке";
	protected override string DefaultDescription => "Задает целевую точку для перемещения.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(MARKER_ID_OPTION).WithDisplayName("MarkerId");
		context.AddOption<float>(ARRIVAL_DISTANCE_OPTION).WithDisplayName("ArrivalDistance").WithDefaultValue(2f);
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
