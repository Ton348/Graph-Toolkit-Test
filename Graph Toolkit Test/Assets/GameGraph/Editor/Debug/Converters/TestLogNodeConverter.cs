using GraphCore.Editor;
using UnityEngine.Scripting.APIUpdating;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GraphCore.Runtime;
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

		string message = CommonGraphImporter.GetOptionValue<string>(model, TestLogNodeModel.MessageOption);

		runtimeNode = new TestLogNode
		{
			message = string.IsNullOrWhiteSpace(message)
				? "TEST NODE WORKS"
				: message
		};

		return true;
	}
}
