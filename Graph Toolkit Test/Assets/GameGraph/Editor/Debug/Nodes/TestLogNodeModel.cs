using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using UnityEngine.Scripting.APIUpdating;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Game1.Graph.Editor", sourceClassName: "TestLogNodeModel")]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class TestLogNodeModel : GameGraphEditorNode
{
	public const string MessageOption = "Message";

	protected override string DefaultTitle => "Test Log";
	protected override string DefaultDescription => "Тестовая нода для проверки Game1.Graph pipeline.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		AddStringOption(context, MessageOption, "Message");
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
