using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestAssignSupplierNodeConverter : GameGraphNodeConverterBase<RequestAssignSupplierNodeModel, RequestAssignSupplierNode>
{
	protected override bool TryConvert(RequestAssignSupplierNodeModel editorNodeModel, out RequestAssignSupplierNode runtimeNode)
	{
		runtimeNode = new RequestAssignSupplierNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestAssignSupplierNodeModel.LOT_ID_OPTION),
			supplierId = GetOptionValue<string>(editorNodeModel, RequestAssignSupplierNodeModel.SUPPLIER_ID_OPTION)
		};
		return true;
	}
}

