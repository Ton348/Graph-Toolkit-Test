using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class RequestSetBusinessMarkupNodeConverter : GameGraphNodeConverterBase<RequestSetBusinessMarkupNodeModel
		, RequestSetBusinessMarkupNode>
	{
		protected override bool TryConvert(
			RequestSetBusinessMarkupNodeModel editorNodeModel,
			out RequestSetBusinessMarkupNode runtimeNode)
		{
			runtimeNode = new RequestSetBusinessMarkupNode
			{
				lotId = GetOptionValue<string>(editorNodeModel, RequestSetBusinessMarkupNodeModel.LotIdOption),
				markupPercent = GetOptionValue(editorNodeModel, RequestSetBusinessMarkupNodeModel.MarkupPercentOption, 0)
			};
			return true;
		}
	}
}