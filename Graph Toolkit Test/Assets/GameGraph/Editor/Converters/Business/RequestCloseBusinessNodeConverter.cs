using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestCloseBusinessNodeConverter : GameGraphNodeConverterBase<RequestCloseBusinessNodeModel, RequestCloseBusinessNode>
{
	protected override bool TryConvert(RequestCloseBusinessNodeModel editorNodeModel, out RequestCloseBusinessNode runtimeNode)
	{
		runtimeNode = new RequestCloseBusinessNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestCloseBusinessNodeModel.LotIdOption)
		};
		return true;
	}
}

