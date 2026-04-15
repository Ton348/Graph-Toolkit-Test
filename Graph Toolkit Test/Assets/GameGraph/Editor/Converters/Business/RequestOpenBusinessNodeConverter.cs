using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestOpenBusinessNodeConverter : GameGraphNodeConverterBase<RequestOpenBusinessNodeModel, RequestOpenBusinessNode>
{
	protected override bool TryConvert(RequestOpenBusinessNodeModel editorNodeModel, out RequestOpenBusinessNode runtimeNode)
	{
		runtimeNode = new RequestOpenBusinessNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestOpenBusinessNodeModel.LotIdOption)
		};
		return true;
	}
}

