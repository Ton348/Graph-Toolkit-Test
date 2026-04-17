using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Graph.Core.Editor
{
	[Serializable]
	[Graph(AssetExtension)]
	public sealed class CommonGraphEditorGraph : Unity.GraphToolkit.Editor.Graph
	{
		internal const string AssetExtension = "basegraph";

		[MenuItem("Assets/Create/Base Graph")]
		private static void CreateAssetFile()
		{
			GraphDatabase.PromptInProjectBrowserToCreateNewAsset<CommonGraphEditorGraph>("Base Graph");
		}
	}
}