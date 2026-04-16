using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.UI;
using System.Collections.Generic;
using System.Threading;

namespace GraphCore.Runtime.Executors.UI
{
	public sealed class ChoiceNodeExecutor : BaseGraphNodeExecutor<ChoiceNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(ChoiceNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (node.options == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault($"ChoiceNode '{node.Id}' options list is null.", GraphNodeExecutionErrorType.InvalidNode));
			}

			List<ChoiceOption> validOptions = new List<ChoiceOption>();
			for (int i = 0; i < node.options.Count; i++)
			{
				ChoiceOption option = node.options[i];
				if (option != null && !string.IsNullOrWhiteSpace(option.nextNodeId) && !string.IsNullOrWhiteSpace(option.label))
				{
					validOptions.Add(option);
				}
			}

			if (validOptions.Count == 0)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault($"ChoiceNode '{node.Id}' has no valid options.", GraphNodeExecutionErrorType.InvalidTransition));
			}

			if (context.ChoiceService == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(validOptions[0].nextNodeId));
			}

			return ExecuteWithServiceAsync(validOptions, context.ChoiceService, cancellationToken);
		}

		private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(List<ChoiceOption> validOptions, IGraphChoiceService choiceService, CancellationToken cancellationToken)
		{
			List<GraphChoiceEntry> entries = new List<GraphChoiceEntry>(validOptions.Count);
			for (int i = 0; i < validOptions.Count; i++)
			{
				entries.Add(new GraphChoiceEntry(validOptions[i].label));
			}

			int selectedIndex = await choiceService.ShowAsync(entries, cancellationToken);
			if (selectedIndex < 0 || selectedIndex >= validOptions.Count)
			{
				return GraphNodeExecutionResult.Fault($"Choice service returned invalid index: {selectedIndex}.", GraphNodeExecutionErrorType.ServiceFailure);
			}

			return GraphNodeExecutionResult.ContinueTo(validOptions[selectedIndex].nextNodeId);
		}
	}
}
