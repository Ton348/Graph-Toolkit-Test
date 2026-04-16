using System.Collections.Generic;
using System.Text;
using Game1.Graph.Runtime;
using Game1.Graph.Runtime.Infrastructure.Validation;
using GraphCore.Editor;
using GraphCore.Runtime;
using UnityEngine;

namespace Game1.Graph.Editor.Infrastructure.Validation
{
	public static class GameGraphBuildValidationBridge
	{
		public static bool ValidateBeforeBuild(
			CommonGraphEditorGraph editorGraph,
			CommonGraph runtimeGraph,
			string editorGraphPath,
			GameGraphValidationComposition validationComposition)
		{
			if (runtimeGraph == null)
			{
				return false;
			}

			if (validationComposition == null || validationComposition.ValidatorRegistry == null)
			{
				return true;
			}

			List<GameGraphNode> gameNodes = CollectGameNodes(runtimeGraph.nodes);
			if (gameNodes.Count == 0)
			{
				return true;
			}

			GameGraphValidationResult result = validationComposition.ValidatorRegistry.ValidateAll(gameNodes);
			if (!result.HasErrors && !result.HasWarnings)
			{
				return true;
			}

			string graphName = editorGraph != null ? editorGraph.name : editorGraphPath;
			string report = BuildReport(graphName, result);

			if (result.HasErrors)
			{
				Debug.LogError(report);
				return false;
			}

			return true;
		}

		private static List<GameGraphNode> CollectGameNodes(List<BaseGraphNode> nodes)
		{
			var result = new List<GameGraphNode>();
			if (nodes == null)
			{
				return result;
			}

			for (var i = 0; i < nodes.Count; i++)
			{
				if (nodes[i] is GameGraphNode gameNode)
				{
					result.Add(gameNode);
				}
			}

			return result;
		}

		private static string BuildReport(string graphName, GameGraphValidationResult result)
		{
			var builder = new StringBuilder();
			builder.Append("[GameGraphValidation] Graph '")
				.Append(graphName)
				.Append("': errors=")
				.Append(result.ErrorCount)
				.Append(", warnings=")
				.Append(result.WarningCount);

			IReadOnlyList<GameGraphValidationIssue> issues = result.Issues;
			for (var i = 0; i < issues.Count; i++)
			{
				GameGraphValidationIssue issue = issues[i];
				builder.Append("\n - [")
					.Append(issue.severity)
					.Append("] ")
					.Append(issue.nodeType ?? "<unknown>")
					.Append("(")
					.Append(issue.nodeId ?? "<no-id>")
					.Append(")")
					.Append(".")
					.Append(issue.fieldName ?? "<field>")
					.Append(": ")
					.Append(issue.message ?? string.Empty);
			}

			return builder.ToString();
		}
	}
}