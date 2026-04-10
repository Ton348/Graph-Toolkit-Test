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

public sealed class QuestStateConditionNodeExecutor : GraphNodeExecutor<QuestStateConditionNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(QuestStateConditionNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context.QuestService == null)
		{
			Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} QuestStateCondition fallback: quest='{node.questId}', expected='{node.state}'");
			Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Quest service is not registered. Using false branch fallback for condition node '{node.Id}'.");
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.falseNodeId));
		}

		return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
	}

	private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(QuestStateConditionNode node, IGraphQuestService questService, CancellationToken cancellationToken)
	{
		QuestState actualState = await questService.GetQuestStateAsync(node.questId, cancellationToken);
		bool isMatch = actualState == node.state;
		return GraphNodeExecutionResult.ContinueTo(isMatch ? node.trueNodeId : node.falseNodeId);
	}
}
