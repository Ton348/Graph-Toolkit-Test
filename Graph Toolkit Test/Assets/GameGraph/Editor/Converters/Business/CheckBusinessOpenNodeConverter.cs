using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class CheckBusinessOpenNodeConverter : GameGraphNodeConverterBase<CheckBusinessOpenNodeModel, CheckBusinessOpenNode>
{
	protected override bool TryConvert(CheckBusinessOpenNodeModel editorNodeModel, out CheckBusinessOpenNode runtimeNode)
	{
		runtimeNode = new CheckBusinessOpenNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, CheckBusinessOpenNodeModel.LotIdOption)
		};
		return true;
	}
}

