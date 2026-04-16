using Game1.Graph.Runtime;
using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

using Game1.Graph.Runtime.Infrastructure;
namespace Game1.Graph.Editor.Templates
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public abstract class GameGraphConditionNodeModel : GameGraphTrueFalseNodeModel
	{
		protected override string defaultTitle => "Game Condition Node";
		protected override string defaultDescription => "Base template for game condition node with true/false branching.";

		protected virtual string conditionOptionKey => GameGraphOptionNames.TargetId;

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			AddStringOption(context, conditionOptionKey, "Condition");
		}
	}
}