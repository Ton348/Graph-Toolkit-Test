using Graph.Core.Editor;
using UnityEngine.Scripting.APIUpdating;

[GameGraphNodeConverter]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Game1.Graph.Editor", sourceClassName: "TestLogNodeConverter")]
public sealed class TestLogNodeConverter : IGameGraphNodeConverter
{
	public bool CanConvert(object editorNodeModel)
	{
		return editorNodeModel is TestLogNodeModel;
	}

	public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
	{
		runtimeNode = null;

		if (editorNodeModel is not TestLogNodeModel model)
		{
			return false;
		}

		string message = CommonGraphImporter.GetOptionValue<string>(model, TestLogNodeModel.MESSAGE_OPTION);

		runtimeNode = new TestLogNode
		{
			message = string.IsNullOrWhiteSpace(message)
				? "TEST NODE WORKS"
				: message
		};

		return true;
	}
}
