using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeConverter]
public sealed class CheckBusinessExistsNodeConverter : GameGraphNodeConverterBase<CheckBusinessExistsNodeModel, CheckBusinessExistsNode>
{
	protected override bool TryConvert(CheckBusinessExistsNodeModel editorNodeModel, out CheckBusinessExistsNode runtimeNode)
	{
		runtimeNode = new CheckBusinessExistsNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, CheckBusinessExistsNodeModel.LotIdOption)
		};
		return true;
	}
}

