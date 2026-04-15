using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class RequestInstallBusinessModuleNodeConverter : GameGraphNodeConverterBase<RequestInstallBusinessModuleNodeModel, RequestInstallBusinessModuleNode>
{
	protected override bool TryConvert(RequestInstallBusinessModuleNodeModel editorNodeModel, out RequestInstallBusinessModuleNode runtimeNode)
	{
		runtimeNode = new RequestInstallBusinessModuleNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestInstallBusinessModuleNodeModel.LOT_ID_OPTION),
			moduleId = GetOptionValue<string>(editorNodeModel, RequestInstallBusinessModuleNodeModel.MODULE_ID_OPTION)
		};
		return true;
	}
}

