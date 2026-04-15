using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class CheckContactKnownNodeConverter : GameGraphNodeConverterBase<CheckContactKnownNodeModel, CheckContactKnownNode>
{
	protected override bool TryConvert(CheckContactKnownNodeModel editorNodeModel, out CheckContactKnownNode runtimeNode)
	{
		runtimeNode = new CheckContactKnownNode
		{
			contactId = GetOptionValue<string>(editorNodeModel, CheckContactKnownNodeModel.CONTACT_ID_OPTION)
		};
		return true;
	}
}

