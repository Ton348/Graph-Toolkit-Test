using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;
[GameGraphNodeExecutorAttribute]
public sealed class CheckContactKnownNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckContactKnownNode>
{
	protected override UniTask<bool> EvaluateConditionAsync(CheckContactKnownNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			&& bootstrap.BusinessStateSyncService != null
			&& bootstrap.BusinessStateSyncService.HasKnownContact(node.contactId);
		return UniTask.FromResult(result);
	}
}

