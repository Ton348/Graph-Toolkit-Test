using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestBuyBuildingNodeConverter : GameGraphNodeConverterBase<RequestBuyBuildingNodeModel, RequestBuyBuildingNode>
{
	protected override bool TryConvert(RequestBuyBuildingNodeModel editorNodeModel, out RequestBuyBuildingNode runtimeNode)
	{
		runtimeNode = new RequestBuyBuildingNode
		{
			buildingId = GetOptionValue<string>(editorNodeModel, RequestBuyBuildingNodeModel.BUILDING_ID_OPTION),
			questAction = GetOptionValue(editorNodeModel, RequestBuyBuildingNodeModel.QUEST_ACTION_OPTION, QuestActionType.None),
			questId = GetOptionValue<string>(editorNodeModel, RequestBuyBuildingNodeModel.QUEST_ID_OPTION)
		};
		return true;
	}
}

