using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class CheckBusinessOpenNodeConverter : GameGraphNodeConverterBase<CheckBusinessOpenNodeModel, CheckBusinessOpenNode>
{
	protected override bool TryConvert(CheckBusinessOpenNodeModel editorNodeModel, out CheckBusinessOpenNode runtimeNode)
	{
		runtimeNode = new CheckBusinessOpenNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, CheckBusinessOpenNodeModel.LOT_ID_OPTION)
		};
		return true;
	}
}

