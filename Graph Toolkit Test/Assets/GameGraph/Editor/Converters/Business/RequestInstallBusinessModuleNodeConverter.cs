using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
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

