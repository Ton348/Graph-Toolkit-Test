using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeConverter]
public sealed class RequestSetBusinessOpenNodeConverter : GameGraphNodeConverterBase<RequestSetBusinessOpenNodeModel, RequestSetBusinessOpenNode>
{
	protected override bool TryConvert(RequestSetBusinessOpenNodeModel editorNodeModel, out RequestSetBusinessOpenNode runtimeNode)
	{
		runtimeNode = new RequestSetBusinessOpenNode
		{
			lotId = GetOptionValue<string>(editorNodeModel, RequestSetBusinessOpenNodeModel.LotIdOption),
			open = GetOptionValue(editorNodeModel, RequestSetBusinessOpenNodeModel.OpenOption, false)
		};
		return true;
	}
}

