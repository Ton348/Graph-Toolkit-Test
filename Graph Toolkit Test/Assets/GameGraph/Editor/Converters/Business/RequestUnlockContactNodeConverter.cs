using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestUnlockContactNodeConverter : GameGraphNodeConverterBase<RequestUnlockContactNodeModel, RequestUnlockContactNode>
{
	protected override bool TryConvert(RequestUnlockContactNodeModel editorNodeModel, out RequestUnlockContactNode runtimeNode)
	{
		runtimeNode = new RequestUnlockContactNode
		{
			contactId = GetOptionValue<string>(editorNodeModel, RequestUnlockContactNodeModel.CONTACT_ID_OPTION)
		};
		return true;
	}
}
