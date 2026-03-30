using System;
using System.Collections.Generic;
using UnityEngine;

public enum GraphDebugFilterMode
{
    All,
    ErrorsOnly
}

public sealed class GraphDebugService
{
    private readonly List<GraphExecutionEvent> history = new List<GraphExecutionEvent>();

    public bool Enabled { get; set; } = true;
    public bool IncludeContextSnapshots { get; set; } = true;
    public bool LogToConsole { get; set; } = false;
    public GraphDebugFilterMode FilterMode { get; set; } = GraphDebugFilterMode.All;
    public int MaxEvents { get; set; } = 200;

    public string CurrentNodeId { get; private set; }
    public GraphExecutionEvent LatestEvent { get; private set; }
    public IReadOnlyList<GraphExecutionEvent> History => history;

    public event Action<GraphExecutionEvent> EventLogged;

    public void LogNodeStart(string graphId, BusinessQuestNode node, GraphExecutionContext context)
    {
        Log(graphId, node, GraphExecutionEventType.Start, null, null, null, context, null);
        if (node != null)
        {
            CurrentNodeId = node.id;
        }
    }

    public void LogNodeSuccess(string graphId, BusinessQuestNode node, GraphExecutionContext context, string message = null, ServerActionResult serverResult = null, string nextNodeId = null)
    {
        Log(graphId, node, GraphExecutionEventType.Success, message, serverResult, nextNodeId, context, null);
    }

    public void LogNodeFail(string graphId, BusinessQuestNode node, GraphExecutionContext context, string message = null, ServerActionResult serverResult = null, string nextNodeId = null)
    {
        Log(graphId, node, GraphExecutionEventType.Fail, message, serverResult, nextNodeId, context, null);
    }

    private void Log(
        string graphId,
        BusinessQuestNode node,
        GraphExecutionEventType eventType,
        string message,
        ServerActionResult serverResult,
        string nextNodeId,
        GraphExecutionContext context,
        string fallbackNodeName)
    {
        if (!Enabled)
        {
            return;
        }

        if (FilterMode == GraphDebugFilterMode.ErrorsOnly && eventType != GraphExecutionEventType.Fail)
        {
            return;
        }

        string nodeId = node != null ? node.id : string.Empty;
        string nodeName = node != null
            ? (!string.IsNullOrEmpty(node.Title) ? node.Title : node.GetType().Name)
            : (fallbackNodeName ?? "Unknown");

        GraphContextSnapshot snapshot = IncludeContextSnapshots ? GraphContextSnapshot.Capture(context) : null;
        GraphExecutionEvent evt = new GraphExecutionEvent(
            graphId,
            nodeId,
            nodeName,
            nextNodeId,
            eventType,
            Time.unscaledTime,
            message,
            serverResult,
            snapshot);

        LatestEvent = evt;
        history.Add(evt);
        TrimHistory();
        EventLogged?.Invoke(evt);

        if (LogToConsole)
        {
            string extra = string.IsNullOrEmpty(message) ? string.Empty : $" ({message})";
            Debug.Log($"[GraphDebug] {evt.EventType} {evt.NodeName} [{evt.NodeId}]{extra}");
        }
    }

    private void TrimHistory()
    {
        if (MaxEvents <= 0)
        {
            history.Clear();
            return;
        }

        int overflow = history.Count - MaxEvents;
        if (overflow > 0)
        {
            history.RemoveRange(0, overflow);
        }
    }

    public void Clear()
    {
        history.Clear();
        LatestEvent = null;
        CurrentNodeId = null;
    }
}
