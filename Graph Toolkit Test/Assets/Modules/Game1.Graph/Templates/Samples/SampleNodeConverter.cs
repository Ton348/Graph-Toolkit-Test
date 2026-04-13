[GameGraphNodeConverter]
public sealed class SampleNodeConverter : GameGraphNodeConverterBase<SampleNodeModel, SampleNode>
{
	protected override bool TryConvert(SampleNodeModel editorNodeModel, out SampleNode runtimeNode)
	{
		runtimeNode = new SampleNode
		{
			enabled = GetBoolOption(editorNodeModel, SampleNodeModel.ENABLED_OPTION, true)
		};

		return true;
	}
}
