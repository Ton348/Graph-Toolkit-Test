using System.Collections.Generic;
using Game1.Graph.Runtime.Infrastructure.Validation;

namespace Game1.Graph.Runtime.Validation
{
	public static class GameGraphValidationHelpers
	{
		public static bool ValidateNodeId(string nodeId, string fieldName, ICollection<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(nodeId))
			{
				return true;
			}

			AddError(errors, $"Field '{fieldName}' is required.");
			return false;
		}

		public static bool ValidateAnyNodeId(
			ICollection<string> errors,
			params (string fieldName, string nodeId)[] fields)
		{
			if (fields == null || fields.Length == 0)
			{
				AddError(errors, "No branch fields were provided for validation.");
				return false;
			}

			for (var i = 0; i < fields.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(fields[i].nodeId))
				{
					return true;
				}
			}

			AddError(errors, "At least one branch target must be configured.");
			return false;
		}

		public static bool ValidateRequiredString(string value, string fieldName, ICollection<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				return true;
			}

			AddError(errors, $"Field '{fieldName}' is required.");
			return false;
		}

		public static bool ValidateNodeId(
			string nodeId,
			GameGraphNode node,
			string fieldName,
			GameGraphValidationResult result)
		{
			if (!string.IsNullOrWhiteSpace(nodeId))
			{
				return true;
			}

			result?.AddError(node, fieldName, $"Field '{fieldName}' is required.");
			return false;
		}

		public static bool ValidateRequiredString(
			string value,
			GameGraphNode node,
			string fieldName,
			GameGraphValidationResult result)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				return true;
			}

			result?.AddError(node, fieldName, $"Field '{fieldName}' is required.");
			return false;
		}

		private static void AddError(ICollection<string> errors, string message)
		{
			if (errors == null)
			{
				return;
			}

			errors.Add(message);
		}
	}
}