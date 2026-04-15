using System;
using UnityEngine.Scripting.APIUpdating;
using Game1.Graph.Runtime;

[Serializable]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Game1.Graph.Runtime", sourceClassName: "TestLogNode")]
public sealed class TestLogNode : GameGraphNode
{
	public string message;
}
