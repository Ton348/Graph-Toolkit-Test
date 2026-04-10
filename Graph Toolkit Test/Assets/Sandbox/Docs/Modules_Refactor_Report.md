Modules Refactor Report

1. Folder Moves
	- Assets/GraphCore -> Assets/Modules/Graph.Core
	- Assets/Game1Graph -> Assets/Modules/Game1.Graph

2. asmdef Renames
	- Assets/Modules/Graph.Core/Runtime/GraphCore.Runtime.asmdef -> Assets/Modules/Graph.Core/Runtime/Graph.Core.Runtime.asmdef
	- Assets/Modules/Graph.Core/Editor/GraphCore.Editor.asmdef -> Assets/Modules/Graph.Core/Editor/Graph.Core.Editor.asmdef
	- Assets/Modules/Game1.Graph/Runtime/Game1Graph.Runtime.asmdef -> Assets/Modules/Game1.Graph/Runtime/Game1.Graph.Runtime.asmdef
	- GraphCore.Runtime -> Graph.Core.Runtime
	- GraphCore.Editor -> Graph.Core.Editor
	- Game1Graph.Runtime -> Game1.Graph.Runtime

3. Class Renames
	- BaseGraph -> CommonGraph
	- BaseGraphRunner -> CommonGraphRunner
	- BaseGraphEditorGraph -> CommonGraphEditorGraph
	- BaseGraphEditorNode -> CommonGraphEditorNode
	- BaseGraphImporter -> CommonGraphImporter
	- BaseGraphRuntimeExporter -> CommonGraphRuntimeExporter
	- BaseGraphRuntimeAutoBuilder -> CommonGraphRuntimeAutoBuilder
	- BaseGraphValidator -> CommonGraphValidator
	- BaseGraphRuntimeComposition -> CommonGraphRuntimeComposition

4. File Splits
	- Assets/Modules/Graph.Core/Runtime/BaseNodeExecutors.cs
	- classes extracted:
		- BaseNodeExecutorConstants
		- StartNodeExecutor
		- FinishNodeExecutor
		- LogNodeExecutor
		- DelayNodeExecutor
		- RandomNodeExecutor
		- DialogueNodeExecutor
		- ChoiceNodeExecutor
		- MapMarkerNodeExecutor
		- PlayCutsceneNodeExecutor
		- CheckpointNodeExecutor
		- StartQuestNodeExecutor
		- CompleteQuestNodeExecutor
		- QuestStateConditionNodeExecutor
	- new files created: one per class, original aggregator removed

	- Assets/Modules/Graph.Core/Runtime/IGraphNodeExecutor.cs
	- classes extracted:
		- IGraphNodeExecutor (kept)
		- GraphNodeExecutor<TNode> -> Assets/Modules/Graph.Core/Runtime/GraphNodeExecutor.cs

	- Assets/Modules/Graph.Core/Runtime/GraphContextKey.cs
	- classes extracted:
		- GraphContextKey (kept)
		- GraphContextKey<T> -> Assets/Modules/Graph.Core/Runtime/GraphContextKeyT.cs

	- Assets/Modules/Graph.Core/Runtime/GraphNodeExecutionResult.cs
	- classes extracted:
		- GraphNodeExecutionSignal -> GraphNodeExecutionSignal.cs
		- GraphNodeExecutionErrorType -> GraphNodeExecutionErrorType.cs
		- GraphNodeExecutionResult -> GraphNodeExecutionResult.cs

	- Assets/Modules/Graph.Core/Runtime/GraphValidationIssue.cs
	- classes extracted:
		- GraphValidationSeverity -> GraphValidationSeverity.cs
		- GraphValidationIssue -> GraphValidationIssue.cs

	- Assets/Modules/Graph.Core/Runtime/GraphExecutionContext.cs
	- types extracted:
		- IGraphDialogueService.cs
		- GraphChoiceEntry.cs
		- IGraphChoiceService.cs
		- IGraphMapMarkerService.cs
		- IGraphCutsceneService.cs
		- IGraphCheckpointService.cs
		- IGraphQuestService.cs
		- IGraphRuntimeServices.cs
		- GraphRuntimeServices.cs
		- GraphExecutionContext.cs

	- Assets/Modules/Game1.Graph/Runtime/Business/BusinessRuntimeApi.cs
	- classes extracted to one-file-per-class wrappers and aggregator removed:
		- CheckBusinessExistsNode.cs
		- CheckBusinessModuleInstalledNode.cs
		- CheckBusinessOpenNode.cs
		- CheckContactKnownNode.cs
		- RequestBuyBuildingNode.cs
		- RequestRentBusinessNode.cs
		- RequestAssignBusinessTypeNode.cs
		- RequestInstallBusinessModuleNode.cs
		- RequestAssignSupplierNode.cs
		- RequestHireBusinessWorkerNode.cs
		- RequestOpenBusinessNode.cs
		- RequestCloseBusinessNode.cs
		- RequestSetBusinessMarkupNode.cs
		- RequestSetBusinessOpenNode.cs
		- RequestTradeOfferNode.cs
		- RequestUnlockContactNode.cs

5. Tabs / Formatting
	- confirmed: yes (leading indentation in Assets/Modules/Graph.Core/** and Assets/Modules/Game1.Graph/** converted to tabs)

6. Empty Folders Removed
	- Assets/Modules/Game1.Graph/Editor
	- Assets/GraphCore.meta (old root meta removed)
	- Assets/Game1Graph.meta (old root meta removed)

7. Compile Result
	- has errors (Unity compile not executed from CLI in this step)

8. Final Structure

Graph.Core
	- Runtime
	- Editor

Game1.Graph
	- Runtime
	- Editor (removed as empty)
