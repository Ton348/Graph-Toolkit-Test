using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class RequestHireBusinessWorkerNodeConverter : GameGraphNodeConverterBase<RequestHireBusinessWorkerNodeModel, RequestHireBusinessWorkerNode>
{
	protected override bool TryConvert(RequestHireBusinessWorkerNodeModel editorNodeModel, out RequestHireBusinessWorkerNode runtimeNode)
	{
		runtimeNode = new RequestHireBusinessWorkerNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestHireBusinessWorkerNodeModel.LOT_ID_OPTION),
			roleId = GetOptionValue<string>(editorNodeModel, RequestHireBusinessWorkerNodeModel.ROLE_ID_OPTION),
			contactId = GetOptionValue<string>(editorNodeModel, RequestHireBusinessWorkerNodeModel.CONTACT_ID_OPTION)
		};
		return true;
	}
}

