using Game1.Graph.Runtime;

namespace Game1.Graph.Editor.Infrastructure
{
	public interface IGameGraphNodeConverter
	{
		bool CanConvert(object editorNodeModel);
		bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode);
	}
}
