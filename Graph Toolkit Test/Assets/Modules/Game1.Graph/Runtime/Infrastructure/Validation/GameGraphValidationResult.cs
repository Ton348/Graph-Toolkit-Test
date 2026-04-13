using System.Collections.Generic;

public sealed class GameGraphValidationResult
{
	private readonly List<GameGraphValidationIssue> m_issues = new List<GameGraphValidationIssue>();

	public IReadOnlyList<GameGraphValidationIssue> Issues => m_issues;
	public bool HasErrors
	{
		get
		{
			for (int i = 0; i < m_issues.Count; i++)
			{
				if (m_issues[i].severity == GameGraphValidationIssueSeverity.Error)
				{
					return true;
				}
			}

			return false;
		}
	}

	public void AddIssue(GameGraphValidationIssueSeverity severity, GameGraphNode node, string fieldName, string message)
	{
		string nodeId = node?.nodeId;
		string nodeType = node?.GetType().Name;
		m_issues.Add(new GameGraphValidationIssue(severity, nodeId, nodeType, fieldName, message));
	}

	public void AddError(GameGraphNode node, string fieldName, string message)
	{
		AddIssue(GameGraphValidationIssueSeverity.Error, node, fieldName, message);
	}

	public void AddWarning(GameGraphNode node, string fieldName, string message)
	{
		AddIssue(GameGraphValidationIssueSeverity.Warning, node, fieldName, message);
	}
}
