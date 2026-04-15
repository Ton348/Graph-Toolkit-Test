using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class CheckBusinessModuleInstalledNodeConverter : GameGraphNodeConverterBase<CheckBusinessModuleInstalledNodeModel, CheckBusinessModuleInstalledNode>
{
	protected override bool TryConvert(CheckBusinessModuleInstalledNodeModel editorNodeModel, out CheckBusinessModuleInstalledNode runtimeNode)
	{
		runtimeNode = new CheckBusinessModuleInstalledNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, CheckBusinessModuleInstalledNodeModel.LOT_ID_OPTION),
			moduleId = GetOptionValue<string>(editorNodeModel, CheckBusinessModuleInstalledNodeModel.MODULE_ID_OPTION)
		};
		return true;
	}
}

