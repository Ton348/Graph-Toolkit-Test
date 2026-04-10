using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class BaseGraphRunner
{
    private const string LogPrefix = "[BaseGraphRunner]";
    private const int DefaultMaxSteps = 10000;

    private readonly GraphNodeExecutorRegistry m_executorRegistry;

    private BaseGraph m_graph;
    private GraphExecutionContext m_context;
    private CancellationTokenSource m_runCancellationTokenSource;
    private BaseGraphNode m_currentNode;

    public BaseGraphRunner(GraphNodeExecutorRegistry executorRegistry)
    {
        m_executorRegistry = executorRegistry ?? throw new ArgumentNullException(nameof(executorRegistry));
    }

    public bool IsRunning { get; private set; }

    public UniTask RunAsync(BaseGraph graph, GraphExecutionContext context, CancellationToken cancellationToken = default, int maxSteps = DefaultMaxSteps)
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

    private async UniTask RunInternalAsync(BaseGraph graph, GraphExecutionContext context, CancellationToken cancellationToken, int maxSteps)
    {
        if (IsRunning)
        {
            Debug.LogWarning($"{LogPrefix} RunAsync ignored because runner is already active.");
            return;
        }

        if (graph == null)
        {
            Debug.LogError($"{LogPrefix} Graph is null.");
            return;
        }

        if (maxSteps <= 0)
        {
            Debug.LogError($"{LogPrefix} Invalid maxSteps value: {maxSteps}.");
            return;
        }

        m_graph = graph;
        m_context = context ?? new GraphExecutionContext();
        m_context.Set(GraphRuntimeContextKeys.currentGraph, m_graph);
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
            Debug.LogError($"{LogPrefix} Start node '{m_graph.startNodeId}' not found.", m_graph);
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
                    Debug.LogError($"{LogPrefix} Max step limit exceeded ({maxSteps}). Potential runaway execution near node '{m_currentNode.Id}'.", m_graph);
                    return;
                }

                if (!m_executorRegistry.TryGetExecutor(m_currentNode, out IGraphNodeExecutor executor))
                {
                    Debug.LogError($"{LogPrefix} No executor registered for node type '{m_currentNode.GetType().Name}' (id: '{m_currentNode.Id}').", m_graph);
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
                if (!string.IsNullOrWhiteSpace(executionResult.diagnosticMessage))
                {
                    Debug.Log($"{LogPrefix} Stop: {executionResult.diagnosticMessage}");
                }

                return false;
            }

            case GraphNodeExecutionSignal.Fault:
            {
                Debug.LogError($"{LogPrefix} Fault ({executionResult.errorType}): {executionResult.diagnosticMessage}", m_graph);
                return false;
            }

            case GraphNodeExecutionSignal.Continue:
            {
                return TryMoveToNextNode(executionResult.nextNodeId);
            }

            default:
            {
                Debug.LogError($"{LogPrefix} Unknown execution signal '{executionResult.signal}'.", m_graph);
                return false;
            }
        }
    }

    private bool TryMoveToNextNode(string nextNodeId)
    {
        if (string.IsNullOrWhiteSpace(nextNodeId))
        {
            Debug.LogError($"{LogPrefix} Execution result has empty next node id. Invalid transition.", m_graph);
            return false;
        }

        if (!m_graph.TryGetNodeById(nextNodeId, out BaseGraphNode nextNode))
        {
            Debug.LogError($"{LogPrefix} Next node '{nextNodeId}' not found.", m_graph);
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
                Debug.LogError($"{LogPrefix} Validation: {issue}", m_graph);
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} Validation: {issue}", m_graph);
            }
        }
    }

    private void Cleanup()
    {
        if (m_context != null)
        {
            m_context.Remove(GraphRuntimeContextKeys.currentGraph);
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
