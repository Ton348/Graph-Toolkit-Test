using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class
		RequestCloseBusinessNodeConverter : GameGraphNodeConverterBase<RequestCloseBusinessNodeModel,
		RequestCloseBusinessNode>
	{
		protected override bool TryConvert(
			RequestCloseBusinessNodeModel editorNodeModel,
			out RequestCloseBusinessNode runtimeNode)
		{
			runtimeNode = new RequestCloseBusinessNode
			{
				lotId = GetOptionValue<string>(editorNodeModel, RequestCloseBusinessNodeModel.LotIdOption)
			};
			return true;
		}
	}
}