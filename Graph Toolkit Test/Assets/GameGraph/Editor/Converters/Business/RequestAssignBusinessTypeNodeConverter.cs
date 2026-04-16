using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Editor.Business;
using GameGraph.Runtime.Business;

namespace GameGraph.Editor.Converters.Business
{
	[GameGraphNodeConverter]
	public sealed class RequestAssignBusinessTypeNodeConverter : GameGraphNodeConverterBase<
		RequestAssignBusinessTypeNodeModel, RequestAssignBusinessTypeNode>
	{
		protected override bool TryConvert(
			RequestAssignBusinessTypeNodeModel editorNodeModel,
			out RequestAssignBusinessTypeNode runtimeNode)
		{
			runtimeNode = new RequestAssignBusinessTypeNode
			{
				lotId = GetOptionValue<string>(editorNodeModel, RequestAssignBusinessTypeNodeModel.LotIdOption),
				businessTypeId =
					GetOptionValue<string>(editorNodeModel, RequestAssignBusinessTypeNodeModel.BusinessTypeIdOption)
			};
			return true;
		}
	}
}