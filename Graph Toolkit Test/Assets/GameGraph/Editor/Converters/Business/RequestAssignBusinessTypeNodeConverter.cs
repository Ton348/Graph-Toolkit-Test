using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestAssignBusinessTypeNodeConverter : GameGraphNodeConverterBase<RequestAssignBusinessTypeNodeModel, RequestAssignBusinessTypeNode>
{
	protected override bool TryConvert(RequestAssignBusinessTypeNodeModel editorNodeModel, out RequestAssignBusinessTypeNode runtimeNode)
	{
		runtimeNode = new RequestAssignBusinessTypeNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestAssignBusinessTypeNodeModel.LOT_ID_OPTION),
			businessTypeId = GetOptionValue<string>(editorNodeModel, RequestAssignBusinessTypeNodeModel.BUSINESS_TYPE_ID_OPTION)
		};
		return true;
	}
}

