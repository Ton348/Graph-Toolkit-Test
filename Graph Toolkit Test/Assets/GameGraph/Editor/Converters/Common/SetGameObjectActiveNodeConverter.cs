using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class SetGameObjectActiveNodeConverter : GameGraphNodeConverterBase<SetGameObjectActiveNodeModel, SetGameObjectActiveNode>
{
	protected override bool TryConvert(SetGameObjectActiveNodeModel model, out SetGameObjectActiveNode runtimeNode)
	{
		runtimeNode = new SetGameObjectActiveNode
		{
			siteId = GetOptionValue<string>(model, SetGameObjectActiveNodeModel.SITE_ID_OPTION),
			visualId = GetOptionValue<string>(model, SetGameObjectActiveNodeModel.VISUAL_ID_OPTION),
			isActive = GetOptionValue(model, SetGameObjectActiveNodeModel.IS_ACTIVE_OPTION, false)
		};
		return true;
	}
}

