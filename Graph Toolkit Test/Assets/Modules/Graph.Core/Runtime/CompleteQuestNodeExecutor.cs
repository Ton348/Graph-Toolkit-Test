using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Cinematics;
using GraphCore.BaseNodes.Runtime.Flow;
using GraphCore.BaseNodes.Runtime.Server;
using GraphCore.BaseNodes.Runtime.UI;
using GraphCore.BaseNodes.Runtime.Utility;
using GraphCore.BaseNodes.Runtime.World;
using UnityEngine;

public sealed class CompleteQuestNodeExecutor : GraphNodeExecutor<CompleteQuestNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CompleteQuestNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context.QuestService == null)
		{
			Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} CompleteQuest fallback: quest='{node.questId}'");
			Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Quest service is not registered. Using fail branch fallback for complete quest node '{node.Id}'.");
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
		}

		return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
	}

	private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(CompleteQuestNode node, IGraphQuestService questService, CancellationToken cancellationToken)
	{
		bool success = await questService.CompleteQuestAsync(node.questId, cancellationToken);
		return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
	}
}
