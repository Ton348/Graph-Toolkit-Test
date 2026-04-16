using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Common;
using GameGraph.Runtime.Common;

namespace GameGraph.Editor.Converters.Common
{
	[GameGraphNodeConverter]
	public sealed class
		SetGameObjectActiveNodeConverter : GameGraphNodeConverterBase<SetGameObjectActiveNodeModel, SetGameObjectActiveNode>
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
}