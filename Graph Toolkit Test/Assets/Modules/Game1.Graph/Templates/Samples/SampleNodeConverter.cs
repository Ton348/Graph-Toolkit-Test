using Game1.Graph.Runtime;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;

using Game1.Graph.Editor.Infrastructure.Converters;
namespace Game1.Graph.Templates.Samples
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
