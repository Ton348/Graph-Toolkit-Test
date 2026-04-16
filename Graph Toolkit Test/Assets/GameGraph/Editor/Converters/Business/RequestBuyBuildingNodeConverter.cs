using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;
using GameGraph.Runtime.Quest;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class
		RequestBuyBuildingNodeConverter : GameGraphNodeConverterBase<RequestBuyBuildingNodeModel, RequestBuyBuildingNode>
	{
		protected override bool TryConvert(
			RequestBuyBuildingNodeModel editorNodeModel,
			out RequestBuyBuildingNode runtimeNode)
		{
			runtimeNode = new RequestBuyBuildingNode
			{
				buildingId = GetOptionValue<string>(editorNodeModel, RequestBuyBuildingNodeModel.BuildingIdOption),
				questAction = GetOptionValue(editorNodeModel, RequestBuyBuildingNodeModel.QuestActionOption,
					QuestActionType.None),
				questId = GetOptionValue<string>(editorNodeModel, RequestBuyBuildingNodeModel.QuestIdOption)
			};
			return true;
		}
	}
}