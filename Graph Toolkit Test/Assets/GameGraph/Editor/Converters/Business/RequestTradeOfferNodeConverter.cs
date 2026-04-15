using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestTradeOfferNodeConverter : GameGraphNodeConverterBase<RequestTradeOfferNodeModel, RequestTradeOfferNode>
{
	protected override bool TryConvert(RequestTradeOfferNodeModel editorNodeModel, out RequestTradeOfferNode runtimeNode)
	{
		runtimeNode = new RequestTradeOfferNode
		{
			buildingId = GetOptionValue<string>(editorNodeModel, RequestTradeOfferNodeModel.BuildingIdOption)
		};
		return true;
	}
}

