using Graph.Core.Editor;

// Copy this file, rename class and types, then map model -> runtime node fields.
// Add [GameGraphNodeConverter] if you want auto-registration.
public abstract class SampleGraphNodeConverter : IGameGraphNodeConverter
{
	public bool CanConvert(object editorNodeModel)
	{
		return editorNodeModel is SampleGraphNodeModel;
	}

	public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
	{
		runtimeNode = null;
		if (editorNodeModel is not SampleGraphNodeModel model)
		{
			return false;
		}

		runtimeNode = new SampleGraphNode
		{
			message = CommonGraphImporter.GetOptionValue<string>(model, SampleGraphNodeModel.MESSAGE_OPTION),
			nextNodeId = null
		};

		return true;
	}
}
