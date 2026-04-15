using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[GameGraphNodeConverter]
public sealed class RequestInstallBusinessModuleNodeConverter : GameGraphNodeConverterBase<RequestInstallBusinessModuleNodeModel, RequestInstallBusinessModuleNode>
{
	protected override bool TryConvert(RequestInstallBusinessModuleNodeModel editorNodeModel, out RequestInstallBusinessModuleNode runtimeNode)
	{
		runtimeNode = new RequestInstallBusinessModuleNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestInstallBusinessModuleNodeModel.LotIdOption),
			moduleId = GetOptionValue<string>(editorNodeModel, RequestInstallBusinessModuleNodeModel.ModuleIdOption)
		};
		return true;
	}
}

