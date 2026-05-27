using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class SelectGoalNodeModel : CommonGraphEditorNode
	{
		public const string NeedTypeOption = "NeedType";
		public const string NeedThresholdOption = "NeedThreshold";
		public const string WorkHourFromOption = "WorkHourFrom";
		public const string WorkHourToOption = "WorkHourTo";
		public const string GoWorkPort = "GoWork";
		public const string GoFoodPort = "GoFood";
		public const string GoHomePort = "GoHome";
		public const string WanderPort = "Wander";

		protected override string defaultTitle => "Выбрать цель";
		protected override string defaultDescription =>
			"Анализирует текущее состояние NPC и выбирает приоритетную цель поведения: работа, еда, отдых и т.д.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.BehaviorNeedType>(NeedTypeOption)
				.WithDisplayName("NeedType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.BehaviorNeedType.Hunger);
			context.AddOption<float>(NeedThresholdOption).WithDisplayName("NeedThreshold").WithDefaultValue(75f);
			context.AddOption<int>(WorkHourFromOption).WithDisplayName("WorkHourFrom").WithDefaultValue(9);
			context.AddOption<int>(WorkHourToOption).WithDisplayName("WorkHourTo").WithDefaultValue(21);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(GoWorkPort).WithDisplayName("GoWork").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(GoFoodPort).WithDisplayName("GoFood").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(GoHomePort).WithDisplayName("GoHome").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(WanderPort).WithDisplayName("Wander").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
