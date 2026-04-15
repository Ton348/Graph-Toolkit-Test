using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestRentBusinessNodeConverter : GameGraphNodeConverterBase<RequestRentBusinessNodeModel, RequestRentBusinessNode>
{
	protected override bool TryConvert(RequestRentBusinessNodeModel editorNodeModel, out RequestRentBusinessNode runtimeNode)
	{
		runtimeNode = new RequestRentBusinessNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestRentBusinessNodeModel.LOT_ID_OPTION)
		};
		return true;
	}
}

