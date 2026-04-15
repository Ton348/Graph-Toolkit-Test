using GraphCore.Editor;

[GameGraphNodeConverter]
public sealed class GoToPointNodeConverter : GameGraphNodeConverterBase<GoToPointNodeModel, GoToPointNode>
{
	protected override bool TryConvert(GoToPointNodeModel model, out GoToPointNode runtimeNode)
	{
		runtimeNode = new GoToPointNode
		{
			markerId = GetOptionValue<string>(model, GoToPointNodeModel.MARKER_ID_OPTION),
			arrivalDistance = GetOptionValue(model, GoToPointNodeModel.ARRIVAL_DISTANCE_OPTION, 2f)
		};
		return true;
	}
}

