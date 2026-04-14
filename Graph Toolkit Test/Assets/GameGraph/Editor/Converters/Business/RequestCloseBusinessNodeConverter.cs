using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class RequestCloseBusinessNodeConverter : GameGraphNodeConverterBase<RequestCloseBusinessNodeModel, RequestCloseBusinessNode>
{
	protected override bool TryConvert(RequestCloseBusinessNodeModel editorNodeModel, out RequestCloseBusinessNode runtimeNode)
	{
		runtimeNode = new RequestCloseBusinessNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestCloseBusinessNodeModel.LOT_ID_OPTION)
		};
		return true;
	}
}

