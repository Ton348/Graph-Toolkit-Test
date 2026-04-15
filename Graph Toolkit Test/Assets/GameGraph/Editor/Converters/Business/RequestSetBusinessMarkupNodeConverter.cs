using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestSetBusinessMarkupNodeConverter : GameGraphNodeConverterBase<RequestSetBusinessMarkupNodeModel, RequestSetBusinessMarkupNode>
{
	protected override bool TryConvert(RequestSetBusinessMarkupNodeModel editorNodeModel, out RequestSetBusinessMarkupNode runtimeNode)
	{
		runtimeNode = new RequestSetBusinessMarkupNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestSetBusinessMarkupNodeModel.LotIdOption),
			markupPercent = GetOptionValue(editorNodeModel, RequestSetBusinessMarkupNodeModel.MarkupPercentOption, 0)
		};
		return true;
	}
}

