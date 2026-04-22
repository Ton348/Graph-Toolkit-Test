using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class RequestBuyTraderItemNodeConverter :
		GameGraphNodeConverterBase<RequestBuyTraderItemNodeModel, RequestBuyTraderItemNode>
	{
		protected override bool TryConvert(
			RequestBuyTraderItemNodeModel editorNodeModel,
			out RequestBuyTraderItemNode runtimeNode)
		{
			runtimeNode = new RequestBuyTraderItemNode
			{
				itemId = GetOptionValue<string>(editorNodeModel, RequestBuyTraderItemNodeModel.ItemIdOption)
			};
			return true;
		}
	}
}
