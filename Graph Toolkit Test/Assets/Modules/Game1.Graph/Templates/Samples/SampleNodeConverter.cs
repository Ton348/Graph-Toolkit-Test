using Game1.Graph.Runtime;

namespace Game1.Graph.Editor
{
	[GameGraphNodeConverter]
	public sealed class SampleNodeConverter : GameGraphNodeConverterBase<SampleNodeModel, SampleNode>
	{
		protected override bool TryConvert(SampleNodeModel editorNodeModel, out SampleNode runtimeNode)
		{
			runtimeNode = new SampleNode
			{
				enabled = GetBoolOption(editorNodeModel, SampleNodeModel.EnabledOption, true)
			};

			return true;
		}
	}
}
