using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeConverter]
public sealed class SetGameObjectActiveNodeConverter : GameGraphNodeConverterBase<SetGameObjectActiveNodeModel, SetGameObjectActiveNode>
{
	protected override bool TryConvert(SetGameObjectActiveNodeModel model, out SetGameObjectActiveNode runtimeNode)
	{
		runtimeNode = new SetGameObjectActiveNode
		{
			siteId = GetOptionValue<string>(model, SetGameObjectActiveNodeModel.SiteIdOption),
			visualId = GetOptionValue<string>(model, SetGameObjectActiveNodeModel.VisualIdOption),
			isActive = GetOptionValue(model, SetGameObjectActiveNodeModel.IsActiveOption, false)
		};
		return true;
	}
}

