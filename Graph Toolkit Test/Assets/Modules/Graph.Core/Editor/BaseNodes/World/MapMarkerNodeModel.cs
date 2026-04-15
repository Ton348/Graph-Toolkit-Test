using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.Editor.BaseNodes.World
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class MapMarkerNodeModel : CommonGraphEditorNode
	{
		public const string MarkerIdOption = "MarkerId";
		public const string TargetOption = "Target";

		protected override string DefaultTitle => "Метка на карте";
		protected override string DefaultDescription => "Добавляет маркер на карту";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);

			context.AddOption<string>(MarkerIdOption)
				.WithDisplayName("MarkerId");

			context.AddOption<string>(TargetOption)
				.WithDisplayName("Target");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
