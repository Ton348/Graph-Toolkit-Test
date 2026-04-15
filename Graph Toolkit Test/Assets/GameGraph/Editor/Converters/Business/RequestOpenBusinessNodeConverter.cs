using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestOpenBusinessNodeConverter : GameGraphNodeConverterBase<RequestOpenBusinessNodeModel, RequestOpenBusinessNode>
{
	protected override bool TryConvert(RequestOpenBusinessNodeModel editorNodeModel, out RequestOpenBusinessNode runtimeNode)
	{
		runtimeNode = new RequestOpenBusinessNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestOpenBusinessNodeModel.LOT_ID_OPTION)
		};
		return true;
	}
}

