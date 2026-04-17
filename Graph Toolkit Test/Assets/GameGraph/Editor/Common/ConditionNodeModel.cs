using System;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Common;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Common
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class ConditionNodeModel : GameGraphTrueFalseNodeModel
	{
		public const string ConditionTypeOption = "ConditionType";
		public const string BuildingIdOption = "BuildingId";
		public const string RequiredMoneyOption = "RequiredMoney";
		public const string PlayerStatTypeOption = "PlayerStatType";
		public const string RequiredStatValueOption = "RequiredStatValue";
		public const string QuestIdOption = "QuestId";

		protected override string defaultTitle => "Проверка условия";
		protected override string defaultDescription => "Проверяет условие и выбирает ветку Истина/Ложь.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<ConditionType>(ConditionTypeOption).WithDisplayName("ConditionType");
			context.AddOption<string>(BuildingIdOption).WithDisplayName("BuildingId");
			context.AddOption<int>(RequiredMoneyOption).WithDisplayName("RequiredMoney");
			context.AddOption<PlayerStatType>(PlayerStatTypeOption).WithDisplayName("PlayerStatType");
			context.AddOption<int>(RequiredStatValueOption).WithDisplayName("RequiredStatValue");
			context.AddOption<string>(QuestIdOption).WithDisplayName("QuestId");
		}
	}
}