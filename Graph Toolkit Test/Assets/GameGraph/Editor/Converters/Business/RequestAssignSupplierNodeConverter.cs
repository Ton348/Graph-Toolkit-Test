using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
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

