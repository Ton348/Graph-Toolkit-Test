using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestAssignBusinessTypeNodeConverter : GameGraphNodeConverterBase<RequestAssignBusinessTypeNodeModel, RequestAssignBusinessTypeNode>
{
	protected override bool TryConvert(RequestAssignBusinessTypeNodeModel editorNodeModel, out RequestAssignBusinessTypeNode runtimeNode)
	{
		runtimeNode = new RequestAssignBusinessTypeNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestAssignBusinessTypeNodeModel.LotIdOption),
			businessTypeId = GetOptionValue<string>(editorNodeModel, RequestAssignBusinessTypeNodeModel.BusinessTypeIdOption)
		};
		return true;
	}
}

