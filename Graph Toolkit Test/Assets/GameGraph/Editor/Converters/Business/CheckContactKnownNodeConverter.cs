using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class CheckContactKnownNodeConverter : GameGraphNodeConverterBase<CheckContactKnownNodeModel, CheckContactKnownNode>
{
	protected override bool TryConvert(CheckContactKnownNodeModel editorNodeModel, out CheckContactKnownNode runtimeNode)
	{
		runtimeNode = new CheckContactKnownNode
		{
			contactId = GetOptionValue<string>(editorNodeModel, CheckContactKnownNodeModel.ContactIdOption)
		};
		return true;
	}
}

