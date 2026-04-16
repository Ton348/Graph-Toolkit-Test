using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class
		RequestOpenBusinessNodeConverter : GameGraphNodeConverterBase<RequestOpenBusinessNodeModel, RequestOpenBusinessNode>
	{
		protected override bool TryConvert(
			RequestOpenBusinessNodeModel editorNodeModel,
			out RequestOpenBusinessNode runtimeNode)
		{
			runtimeNode = new RequestOpenBusinessNode
			{
				lotId = GetOptionValue<string>(editorNodeModel, RequestOpenBusinessNodeModel.LotIdOption)
			};
			return true;
		}
	}
}