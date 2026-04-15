using GraphCore.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeConverter]
public sealed class GoToPointNodeConverter : GameGraphNodeConverterBase<GoToPointNodeModel, GoToPointNode>
{
	protected override bool TryConvert(GoToPointNodeModel model, out GoToPointNode runtimeNode)
	{
		runtimeNode = new GoToPointNode
		{
			markerId = GetOptionValue<string>(model, GoToPointNodeModel.MarkerIdOption),
			arrivalDistance = GetOptionValue(model, GoToPointNodeModel.ArrivalDistanceOption, 2f)
		};
		return true;
	}
}

