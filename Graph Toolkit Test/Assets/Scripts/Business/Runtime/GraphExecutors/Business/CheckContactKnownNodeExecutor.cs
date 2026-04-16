using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
	public sealed class CheckContactKnownNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckContactKnownNode>
	{
		protected override UniTask<bool> EvaluateConditionAsync(
			CheckContactKnownNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			              && bootstrap.BusinessStateSyncService != null
			              && bootstrap.BusinessStateSyncService.HasKnownContact(node.contactId);
			return UniTask.FromResult(result);
		}
	}
}