using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class RequestSetBusinessMarkupNodeConverter : GameGraphNodeConverterBase<RequestSetBusinessMarkupNodeModel, RequestSetBusinessMarkupNode>
{
	protected override bool TryConvert(RequestSetBusinessMarkupNodeModel editorNodeModel, out RequestSetBusinessMarkupNode runtimeNode)
	{
		runtimeNode = new RequestSetBusinessMarkupNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestSetBusinessMarkupNodeModel.LOT_ID_OPTION),
			markupPercent = GetOptionValue(editorNodeModel, RequestSetBusinessMarkupNodeModel.MARKUP_PERCENT_OPTION, 0)
		};
		return true;
	}
}

