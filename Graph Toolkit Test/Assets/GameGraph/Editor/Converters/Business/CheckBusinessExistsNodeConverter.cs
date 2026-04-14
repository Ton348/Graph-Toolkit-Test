using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class CheckBusinessExistsNodeConverter : GameGraphNodeConverterBase<CheckBusinessExistsNodeModel, CheckBusinessExistsNode>
{
	protected override bool TryConvert(CheckBusinessExistsNodeModel editorNodeModel, out CheckBusinessExistsNode runtimeNode)
	{
		runtimeNode = new CheckBusinessExistsNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, CheckBusinessExistsNodeModel.LOT_ID_OPTION)
		};
		return true;
	}
}

