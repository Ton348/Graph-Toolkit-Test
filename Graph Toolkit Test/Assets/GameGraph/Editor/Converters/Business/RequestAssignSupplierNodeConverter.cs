using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestAssignSupplierNodeConverter : GameGraphNodeConverterBase<RequestAssignSupplierNodeModel, RequestAssignSupplierNode>
{
	protected override bool TryConvert(RequestAssignSupplierNodeModel editorNodeModel, out RequestAssignSupplierNode runtimeNode)
	{
		runtimeNode = new RequestAssignSupplierNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestAssignSupplierNodeModel.LotIdOption),
			supplierId = GetOptionValue<string>(editorNodeModel, RequestAssignSupplierNodeModel.SupplierIdOption)
		};
		return true;
	}
}

