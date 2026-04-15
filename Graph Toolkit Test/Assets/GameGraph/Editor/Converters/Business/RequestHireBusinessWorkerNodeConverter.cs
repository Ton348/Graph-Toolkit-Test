using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestHireBusinessWorkerNodeConverter : GameGraphNodeConverterBase<RequestHireBusinessWorkerNodeModel, RequestHireBusinessWorkerNode>
{
	protected override bool TryConvert(RequestHireBusinessWorkerNodeModel editorNodeModel, out RequestHireBusinessWorkerNode runtimeNode)
	{
		runtimeNode = new RequestHireBusinessWorkerNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestHireBusinessWorkerNodeModel.LotIdOption),
			roleId = GetOptionValue<string>(editorNodeModel, RequestHireBusinessWorkerNodeModel.RoleIdOption),
			contactId = GetOptionValue<string>(editorNodeModel, RequestHireBusinessWorkerNodeModel.ContactIdOption)
		};
		return true;
	}
}

