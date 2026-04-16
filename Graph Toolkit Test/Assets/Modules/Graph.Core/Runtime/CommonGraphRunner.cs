using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace GraphCore.Runtime
{
	public sealed class CommonGraphRunner
	{
		private const string s_logPrefix = "[CommonGraphRunner]";
		private const int s_defaultMaxSteps = 10000;

		private readonly GraphNodeExecutorRegistry m_executorRegistry;

		private CommonGraph m_graph;
		private GraphExecutionContext m_context;
		private CancellationTokenSource m_runCancellationTokenSource;
		private BaseGraphNode m_currentNode;

		public CommonGraphRunner(GraphNodeExecutorRegistry executorRegistry)
		{
			m_executorRegistry = executorRegistry ?? throw new ArgumentNullException(nameof(executorRegistry));
		}

		public bool IsRunning { get; private set; }

		public UniTask RunAsync(CommonGraph graph, GraphExecutionContext context, CancellationToken cancellationToken = default, int maxSteps = s_defaultMaxSteps)
		{
			return RunInternalAsync(graph, context, cancellationToken, maxSteps);
		}

		public void Stop()
		{
			CancellationTokenSource cts = m_runCancellationTokenSource;
			if (cts != null && !cts.IsCancellationRequested)
			{
				cts.Cancel();
			}
		}

		private async UniTask RunInternalAsync(CommonGraph graph, GraphExecutionContext context, CancellationToken cancellationToken, int maxSteps)
		{
			if (IsRunning)
			{
				return;
			}

			if (graph == null)
			{
				Debug.LogError($"{s_logPrefix} Graph is null.");
				return;
			}

			if (maxSteps <= 0)
			{
				Debug.LogError($"{s_logPrefix} Invalid maxSteps value: {maxSteps}.");
				return;
			}

			m_graph = graph;
			m_context = context ?? new GraphExecutionContext();
			m_context.CurrentGraph = m_graph;
			m_runCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			CancellationToken runCancellationToken = m_runCancellationTokenSource.Token;

			GraphValidationResult validationResult = m_graph.ValidateGraph();
			if (!validationResult.IsValid)
			{
				LogValidationIssues(validationResult);
				Cleanup();
				return;
			}

			if (!m_graph.TryGetStartNode(out BaseGraphNode startNode))
			{
				Debug.LogError($"{s_logPrefix} Start node '{m_graph.startNodeId}' not found.", m_graph);
				Cleanup();
				return;
			}

			m_currentNode = startNode;
			IsRunning = true;

			int step = 0;
			try
			{
				while (IsRunning && m_currentNode != null)
				{
					runCancellationToken.ThrowIfCancellationRequested();

					step++;
					if (step > maxSteps)
					{
						Debug.LogError($"{s_logPrefix} Max step limit exceeded ({maxSteps}). Potential runaway execution near node '{m_currentNode.Id}'.", m_graph);
						return;
					}

					if (!m_executorRegistry.TryGetExecutor(m_currentNode, out IGraphNodeExecutor executor))
					{
						Debug.LogError($"{s_logPrefix} No executor registered for node type '{m_currentNode.GetType().Name}' (id: '{m_currentNode.Id}').", m_graph);
						return;
					}

					GraphNodeExecutionResult executionResult = await executor.ExecuteAsync(m_currentNode, m_context, runCancellationToken);
					if (!HandleExecutionResult(executionResult))
					{
						return;
					}
				}
			}
			catch (OperationCanceledException)
			{
			}
			finally
			{
				Cleanup();
			}
		}

		private bool HandleExecutionResult(GraphNodeExecutionResult executionResult)
		{
			switch (executionResult.signal)
			{
				case GraphNodeExecutionSignal.Stop:
				{
					return false;
				}

				case GraphNodeExecutionSignal.Fault:
				{
					Debug.LogError($"{s_logPrefix} Fault ({executionResult.errorType}): {executionResult.diagnosticMessage}", m_graph);
					return false;
				}

				case GraphNodeExecutionSignal.Continue:
				{
					return TryMoveToNextNode(executionResult.nextNodeId);
				}

				default:
				{
					Debug.LogError($"{s_logPrefix} Unknown execution signal '{executionResult.signal}'.", m_graph);
					return false;
				}
			}
		}

		private bool TryMoveToNextNode(string nextNodeId)
		{
			if (string.IsNullOrWhiteSpace(nextNodeId))
			{
				Debug.LogError($"{s_logPrefix} Execution result has empty next node id. Invalid transition.", m_graph);
				return false;
			}

			if (!m_graph.TryGetNodeById(nextNodeId, out BaseGraphNode nextNode))
			{
				Debug.LogError($"{s_logPrefix} Next node '{nextNodeId}' not found.", m_graph);
				return false;
			}

			m_currentNode = nextNode;
			return true;
		}

		private void LogValidationIssues(GraphValidationResult validationResult)
		{
			for (int i = 0; i < validationResult.Issues.Count; i++)
			{
				GraphValidationIssue issue = validationResult.Issues[i];
				if (issue.severity == GraphValidationSeverity.Error)
				{
					Debug.LogError($"{s_logPrefix} Validation: {issue}", m_graph);
				}
			}
		}

		private void Cleanup()
		{
			if (m_context != null)
			{
				m_context.CurrentGraph = null;
			}

			IsRunning = false;
			m_currentNode = null;
			m_context = null;
			m_graph = null;

			CancellationTokenSource cancellationTokenSource = m_runCancellationTokenSource;
			m_runCancellationTokenSource = null;

			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Dispose();
			}
		}
	}
}
