using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestBuyBuildingNodeConverter : GameGraphNodeConverterBase<RequestBuyBuildingNodeModel, RequestBuyBuildingNode>
{
	protected override bool TryConvert(RequestBuyBuildingNodeModel editorNodeModel, out RequestBuyBuildingNode runtimeNode)
	{
		runtimeNode = new RequestBuyBuildingNode
		{
			buildingId = GetOptionValue<string>(editorNodeModel, RequestBuyBuildingNodeModel.BuildingIdOption),
			questAction = GetOptionValue(editorNodeModel, RequestBuyBuildingNodeModel.QuestActionOption, QuestActionType.None),
			questId = GetOptionValue<string>(editorNodeModel, RequestBuyBuildingNodeModel.QuestIdOption)
		};
		return true;
	}
}

