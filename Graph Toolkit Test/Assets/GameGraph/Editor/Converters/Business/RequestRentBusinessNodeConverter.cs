using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestRentBusinessNodeConverter : GameGraphNodeConverterBase<RequestRentBusinessNodeModel, RequestRentBusinessNode>
{
	protected override bool TryConvert(RequestRentBusinessNodeModel editorNodeModel, out RequestRentBusinessNode runtimeNode)
	{
		runtimeNode = new RequestRentBusinessNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestRentBusinessNodeModel.LotIdOption)
		};
		return true;
	}
}

