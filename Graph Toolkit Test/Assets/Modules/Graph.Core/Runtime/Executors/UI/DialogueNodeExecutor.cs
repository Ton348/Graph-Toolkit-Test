using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Nodes.UI;

namespace Graph.Core.Runtime.Executors.UI
{
	public sealed class DialogueNodeExecutor : BaseGraphNodeExecutor<DialogueNode>
	{
		protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
			DialogueNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			IGraphDialogueService dialogueService = context.DialogueService;
			if (dialogueService == null)
			{
				return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
			}

			if (!TryGetImmediateChoiceNode(node, context, out ChoiceNode choiceNode))
			{
				await dialogueService.ShowAsync(node.dialogueTitle, node.body, cancellationToken);
				return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
			}

			_ = dialogueService.ShowAsync(node.dialogueTitle, node.body, cancellationToken);
			return await ExecuteImmediateChoiceAsync(choiceNode, context, cancellationToken);
		}

		private static bool TryGetImmediateChoiceNode(
			DialogueNode node,
			GraphExecutionContext context,
			out ChoiceNode choiceNode)
		{
			choiceNode = null;
			if (!context.ImmediateChoiceAfterDialogue)
			{
				return false;
			}

			CommonGraph graph = context.CurrentGraph;
			if (graph == null)
			{
				return false;
			}

			if (!graph.TryGetNodeById(node.nextNodeId, out BaseGraphNode nextNode) ||
			    nextNode is not ChoiceNode foundChoiceNode)
			{
				return false;
			}

			choiceNode = foundChoiceNode;
			return true;
		}

		private static async UniTask<GraphNodeExecutionResult> ExecuteImmediateChoiceAsync(
			ChoiceNode choiceNode,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (choiceNode.options == null)
			{
				return GraphNodeExecutionResult.Fault($"ChoiceNode '{choiceNode.Id}' options list is null.",
					GraphNodeExecutionErrorType.InvalidNode);
			}

			var validOptions = new List<ChoiceOption>();
			for (var i = 0; i < choiceNode.options.Count; i++)
			{
				ChoiceOption option = choiceNode.options[i];
				if (option != null && !string.IsNullOrWhiteSpace(option.nextNodeId) &&
				    !string.IsNullOrWhiteSpace(option.label))
				{
					validOptions.Add(option);
				}
			}

			if (validOptions.Count == 0)
			{
				return GraphNodeExecutionResult.Fault($"ChoiceNode '{choiceNode.Id}' has no valid options.",
					GraphNodeExecutionErrorType.InvalidTransition);
			}

			if (context.ChoiceService == null)
			{
				return GraphNodeExecutionResult.ContinueTo(validOptions[0].nextNodeId);
			}

			var entries = new List<GraphChoiceEntry>(validOptions.Count);
			for (var i = 0; i < validOptions.Count; i++)
			{
				entries.Add(new GraphChoiceEntry(validOptions[i].label));
			}

			int selectedIndex = await context.ChoiceService.ShowAsync(entries, cancellationToken);
			if (selectedIndex < 0 || selectedIndex >= validOptions.Count)
			{
				return GraphNodeExecutionResult.Fault($"Choice service returned invalid index: {selectedIndex}.",
					GraphNodeExecutionErrorType.ServiceFailure);
			}

			return GraphNodeExecutionResult.ContinueTo(validOptions[selectedIndex].nextNodeId);
		}
	}
}