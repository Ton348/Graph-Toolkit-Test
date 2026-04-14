using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class RequestSetBusinessOpenNodeConverter : GameGraphNodeConverterBase<RequestSetBusinessOpenNodeModel, RequestSetBusinessOpenNode>
{
	protected override bool TryConvert(RequestSetBusinessOpenNodeModel editorNodeModel, out RequestSetBusinessOpenNode runtimeNode)
	{
		runtimeNode = new RequestSetBusinessOpenNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestSetBusinessOpenNodeModel.LOT_ID_OPTION),
			open = GetOptionValue(editorNodeModel, RequestSetBusinessOpenNodeModel.OPEN_OPTION, false)
		};
		return true;
	}
}

