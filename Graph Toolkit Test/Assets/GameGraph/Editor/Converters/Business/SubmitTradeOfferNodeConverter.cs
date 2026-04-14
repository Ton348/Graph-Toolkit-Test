using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class SubmitTradeOfferNodeConverter : GameGraphNodeConverterBase<SubmitTradeOfferNodeModel, RequestTradeOfferNode>
{
	protected override bool TryConvert(SubmitTradeOfferNodeModel editorNodeModel, out RequestTradeOfferNode runtimeNode)
	{
		runtimeNode = new RequestTradeOfferNode
		{
			buildingId = GetOptionValue<string>(editorNodeModel, SubmitTradeOfferNodeModel.BUILDING_ID_OPTION)
		};
		return true;
	}
}
