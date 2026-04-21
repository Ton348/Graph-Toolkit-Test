#if UNITY_EDITOR
using Game1.Graph.Editor.Infrastructure.Bootstrap;
using GameGraph.Editor.Converters.Business;
using UnityEditor;

[InitializeOnLoad]
public static class RequestBuyTraderItemNodeRegistration
{
	static RequestBuyTraderItemNodeRegistration()
	{
		if (GameGraphEditorBootstrap.Module?.EditorComposition?.ConverterRegistry == null)
		{
			return;
		}

		GameGraphEditorBootstrap.Module.EditorComposition.RegisterConverter<RequestBuyTraderItemNodeConverter>();
	}
}
#endif
