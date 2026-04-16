using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class
		RequestTradeOfferNodeConverter : GameGraphNodeConverterBase<RequestTradeOfferNodeModel, RequestTradeOfferNode>
	{
		protected override bool TryConvert(
			RequestTradeOfferNodeModel editorNodeModel,
			out RequestTradeOfferNode runtimeNode)
		{
			runtimeNode = new RequestTradeOfferNode
			{
				buildingId = GetOptionValue<string>(editorNodeModel, RequestTradeOfferNodeModel.BuildingIdOption)
			};
			return true;
		}
	}
}