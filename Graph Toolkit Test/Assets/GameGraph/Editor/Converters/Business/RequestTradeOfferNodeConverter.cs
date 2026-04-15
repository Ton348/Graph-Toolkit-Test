using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestTradeOfferNodeConverter : GameGraphNodeConverterBase<RequestTradeOfferNodeModel, RequestTradeOfferNode>
{
	protected override bool TryConvert(RequestTradeOfferNodeModel editorNodeModel, out RequestTradeOfferNode runtimeNode)
	{
		runtimeNode = new RequestTradeOfferNode
		{
			buildingId = GetOptionValue<string>(editorNodeModel, RequestTradeOfferNodeModel.BUILDING_ID_OPTION)
		};
		return true;
	}
}

