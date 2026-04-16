using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class CheckBusinessModuleInstalledNodeConverter : GameGraphNodeConverterBase<
		CheckBusinessModuleInstalledNodeModel, CheckBusinessModuleInstalledNode>
	{
		protected override bool TryConvert(
			CheckBusinessModuleInstalledNodeModel editorNodeModel,
			out CheckBusinessModuleInstalledNode runtimeNode)
		{
			runtimeNode = new CheckBusinessModuleInstalledNode
			{
				lotId = GetOptionValue<string>(editorNodeModel, CheckBusinessModuleInstalledNodeModel.LotIdOption),
				moduleId = GetOptionValue<string>(editorNodeModel, CheckBusinessModuleInstalledNodeModel.ModuleIdOption)
			};
			return true;
		}
	}
}