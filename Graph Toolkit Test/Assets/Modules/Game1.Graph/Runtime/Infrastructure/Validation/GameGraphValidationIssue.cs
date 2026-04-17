namespace Game1.Graph.Runtime.Infrastructure.Validation
{
	public readonly struct GameGraphValidationIssue
	{
		public readonly GameGraphValidationIssueSeverity severity;
		public readonly string nodeId;
		public readonly string nodeType;
		public readonly string fieldName;
		public readonly string message;

		public GameGraphValidationIssue(
			GameGraphValidationIssueSeverity severity,
			string nodeId,
			string nodeType,
			string fieldName,
			string message)
		{
			this.severity = severity;
			this.nodeId = nodeId;
			this.nodeType = nodeType;
			this.fieldName = fieldName;
			this.message = message;
		}
	}
}