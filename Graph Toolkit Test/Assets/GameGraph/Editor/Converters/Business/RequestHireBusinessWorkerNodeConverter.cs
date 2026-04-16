using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
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

