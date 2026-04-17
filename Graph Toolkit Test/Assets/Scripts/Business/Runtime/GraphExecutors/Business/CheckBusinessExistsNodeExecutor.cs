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
	public sealed class CheckBusinessExistsNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckBusinessExistsNode>
	{
		protected override UniTask<bool> EvaluateConditionAsync(
			CheckBusinessExistsNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			              && bootstrap.BusinessStateSyncService != null
			              && bootstrap.BusinessStateSyncService.HasBusiness(node.lotId);
			return UniTask.FromResult(result);
		}
	}
}