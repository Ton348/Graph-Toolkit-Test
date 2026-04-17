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
	public sealed class
		CheckBusinessModuleInstalledNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckBusinessModuleInstalledNode>
	{
		protected override UniTask<bool> EvaluateConditionAsync(
			CheckBusinessModuleInstalledNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			              && bootstrap.BusinessStateSyncService != null
			              && bootstrap.BusinessStateSyncService.HasModule(node.lotId, node.moduleId);
			return UniTask.FromResult(result);
		}
	}
}