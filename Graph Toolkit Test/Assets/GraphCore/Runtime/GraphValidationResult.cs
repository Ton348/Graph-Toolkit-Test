using System;
using System.Collections.Generic;

public sealed class GraphValidationResult
{
    private readonly List<GraphValidationIssue> m_issues = new List<GraphValidationIssue>();
    private List<GraphValidationIssue> m_errors;
    private List<GraphValidationIssue> m_warnings;

    public IReadOnlyList<GraphValidationIssue> Issues => m_issues;
    public bool IsValid => ErrorCount == 0;

    public int ErrorCount => Errors.Count;

    public int WarningCount => Warnings.Count;

    public IReadOnlyList<GraphValidationIssue> Errors
    {
        get
        {
            if (m_errors == null)
            {
                m_errors = FilterIssues(GraphValidationSeverity.Error);
            }

            return m_errors;
        }
    }

    public IReadOnlyList<GraphValidationIssue> Warnings
    {
        get
        {
            if (m_warnings == null)
            {
                m_warnings = FilterIssues(GraphValidationSeverity.Warning);
            }

            return m_warnings;
        }
    }

    public void AddIssue(GraphValidationIssue issue)
    {
        if (issue == null)
        {
            throw new ArgumentNullException(nameof(issue));
        }

        m_issues.Add(issue);
        InvalidateCaches();
    }

    public void AddError(string message, string nodeId = null, string nodeType = null, string fieldName = null, int nodeIndex = -1)
    {
        AddIssue(GraphValidationIssue.Error(message, nodeId, nodeType, fieldName, nodeIndex));
    }

    public void AddWarning(string message, string nodeId = null, string nodeType = null, string fieldName = null, int nodeIndex = -1)
    {
        AddIssue(GraphValidationIssue.Warning(message, nodeId, nodeType, fieldName, nodeIndex));
    }

    private List<GraphValidationIssue> FilterIssues(GraphValidationSeverity severity)
    {
        List<GraphValidationIssue> issues = new List<GraphValidationIssue>();
        for (int i = 0; i < m_issues.Count; i++)
        {
            GraphValidationIssue issue = m_issues[i];
            if (issue.severity == severity)
            {
                issues.Add(issue);
            }
        }

        return issues;
    }

    private void InvalidateCaches()
    {
        m_errors = null;
        m_warnings = null;
    }
}
