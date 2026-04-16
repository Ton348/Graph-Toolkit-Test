using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
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

