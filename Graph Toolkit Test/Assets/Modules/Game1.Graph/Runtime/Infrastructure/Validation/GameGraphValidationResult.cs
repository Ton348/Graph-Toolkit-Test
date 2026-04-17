using System.Collections.Generic;

namespace Game1.Graph.Runtime.Infrastructure.Validation
{
	public sealed class GameGraphValidationResult
	{
		private readonly List<GameGraphValidationIssue> m_issues = new();

		public IReadOnlyList<GameGraphValidationIssue> Issues => m_issues;

		public int ErrorCount
		{
			get
			{
				var count = 0;
				for (var i = 0; i < m_issues.Count; i++)
				{
					if (m_issues[i].severity == GameGraphValidationIssueSeverity.Error)
					{
						count++;
					}
				}

				return count;
			}
		}

		public int WarningCount
		{
			get
			{
				var count = 0;
				for (var i = 0; i < m_issues.Count; i++)
				{
					if (m_issues[i].severity == GameGraphValidationIssueSeverity.Warning)
					{
						count++;
					}
				}

				return count;
			}
		}

		public bool HasErrors => ErrorCount > 0;

		public bool HasWarnings => WarningCount > 0;

		public void AddIssue(
			GameGraphValidationIssueSeverity severity,
			GameGraphNode node,
			string fieldName,
			string message)
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
}