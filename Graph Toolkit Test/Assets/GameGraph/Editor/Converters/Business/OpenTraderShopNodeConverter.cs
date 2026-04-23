using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class OpenTraderShopNodeConverter : GameGraphNodeConverterBase<OpenTraderShopNodeModel, OpenTraderShopNode>
	{
		protected override bool TryConvert(OpenTraderShopNodeModel editorNodeModel, out OpenTraderShopNode runtimeNode)
		{
			runtimeNode = new OpenTraderShopNode
			{
				traderId = GetOptionValue<string>(editorNodeModel, OpenTraderShopNodeModel.TraderIdOption)
			};
			return true;
		}
	}
}
