public interface IGameGraphNodeConverter
{
	bool CanConvert(object editorNodeModel);
	bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode);
}
