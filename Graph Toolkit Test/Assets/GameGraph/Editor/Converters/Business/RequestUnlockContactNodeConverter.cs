using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class
		RequestUnlockContactNodeConverter : GameGraphNodeConverterBase<RequestUnlockContactNodeModel,
		RequestUnlockContactNode>
	{
		protected override bool TryConvert(
			RequestUnlockContactNodeModel editorNodeModel,
			out RequestUnlockContactNode runtimeNode)
		{
			runtimeNode = new RequestUnlockContactNode
			{
				contactId = GetOptionValue<string>(editorNodeModel, RequestUnlockContactNodeModel.ContactIdOption)
			};
			return true;
		}
	}
}