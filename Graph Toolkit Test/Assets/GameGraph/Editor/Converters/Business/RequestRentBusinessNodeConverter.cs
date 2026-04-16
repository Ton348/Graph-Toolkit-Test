using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class
		RequestRentBusinessNodeConverter : GameGraphNodeConverterBase<RequestRentBusinessNodeModel, RequestRentBusinessNode>
	{
		protected override bool TryConvert(
			RequestRentBusinessNodeModel editorNodeModel,
			out RequestRentBusinessNode runtimeNode)
		{
			runtimeNode = new RequestRentBusinessNode
			{
				lotId = GetOptionValue<string>(editorNodeModel, RequestRentBusinessNodeModel.LotIdOption)
			};
			return true;
		}
	}
}