# Final GraphCore / Game1Graph Cleanup Report

## 1. Removed From GraphCore

- `Assets/GraphCore/Editor/QuestEditorGraph.cs`
- `Assets/GraphCore/Editor/QuestEditorGraph.cs.meta`

## 2. Removed From Game1Graph

- `Assets/Game1Graph/Runtime/Common/CommonRuntimeApi.cs`
- `Assets/Game1Graph/Runtime/Common/CommonRuntimeApi.cs.meta`
- `Assets/Game1Graph/Runtime/Quest/QuestRuntimeApi.cs`
- `Assets/Game1Graph/Runtime/Quest/QuestRuntimeApi.cs.meta`
- `Assets/Game1Graph/Runtime/Quest/CheckpointNode.cs`
- `Assets/Game1Graph/Runtime/Quest/CheckpointNode.cs.meta`
- `Assets/Game1Graph/Runtime/Quest/RequestStartQuestNode.cs`
- `Assets/Game1Graph/Runtime/Quest/RequestStartQuestNode.cs.meta`
- `Assets/Game1Graph/Runtime/Quest/RequestCompleteQuestNode.cs`
- `Assets/Game1Graph/Runtime/Quest/RequestCompleteQuestNode.cs.meta`
- `Assets/Game1Graph/Runtime/Quest/RefreshProfileNode.cs`
- `Assets/Game1Graph/Runtime/Quest/RefreshProfileNode.cs.meta`
- `Assets/Game1Graph/Runtime/Quest/QuestCompatibilityNodes.cs`
- `Assets/Game1Graph/Runtime/Quest/QuestCompatibilityNodes.cs.meta`

## 3. Usage Switched

- `CheckpointNode` (legacy Game1Graph) -> `GraphCore.BaseNodes.Runtime.Server.CheckpointNode`
- `RequestStartQuestNode` -> `GraphCore.BaseNodes.Runtime.Server.StartQuestNode`
- `RequestCompleteQuestNode` -> `GraphCore.BaseNodes.Runtime.Server.CompleteQuestNode`
- `StartNode` usage in legacy runner -> `GraphCore.BaseNodes.Runtime.Flow.StartNode`
- `EndNode` usage in legacy runner -> `GraphCore.BaseNodes.Runtime.Flow.FinishNode`
- `DialogueNode` usage in legacy runner -> `GraphCore.BaseNodes.Runtime.UI.DialogueNode`
- `ChoiceNode` usage in legacy runner -> `GraphCore.BaseNodes.Runtime.UI.ChoiceNode`
- `ChoiceOption` usage in legacy runner/UI -> `GraphCore.BaseNodes.Runtime.UI.ChoiceOption`
- `AddMapMarkerNode` usage in legacy runner -> `GraphCore.BaseNodes.Runtime.World.MapMarkerNode`

## 4. Final GraphCore Contents

### Runtime files
- `BaseGraph.cs`
- `BaseGraphNode.cs`
- `BaseGraphRunner.cs`
- `BaseGraphRuntimeComposition.cs`
- `BaseGraphValidator.cs`
- `BaseNodeExecutors.cs`
- `GraphContextKey.cs`
- `GraphExecutionContext.cs`
- `GraphNodeExecutionResult.cs`
- `GraphNodeExecutorRegistry.cs`
- `GraphValidationIssue.cs`
- `GraphValidationResult.cs`
- `IGraphNodeExecutor.cs`
- `BaseNodes/*` (Flow, UI, Utility, World, Cinematics, Server)

### Editor files
- `BaseGraphEditorGraph.cs`
- `BaseGraphEditorNode.cs`
- `BaseGraphImporter.cs`
- `BaseGraphRuntimeExporter.cs`
- `BaseGraphRuntimeAutoBuilder.cs`
- `BaseNodes/*NodeModel.cs`

## 5. Final Game1Graph Contents

### Runtime files
- `BusinessQuestGraph.cs`
- `Common/ConditionNode.cs`
- `Common/ConditionType.cs`
- `Common/PlayerStatType.cs`
- `Common/SkillType.cs`
- `Common/GoToPointNode.cs`
- `Common/SetGameObjectActiveNode.cs`
- `Quest/QuestActionType.cs`
- `Business/*` (all game-specific business request/check nodes)

### Editor files
- No active editor files under `Assets/Game1Graph/Editor` in current tree.

## 6. asmdef Check

- `GraphCore.Runtime`
  - references: `UniTask`
  - no refs to Game1Graph/Sandbox/Prototype
- `GraphCore.Editor`
  - references: `GraphCore.Runtime`, `Unity.GraphToolkit.Editor`, `Unity.GraphToolkit.Common.Editor`, `Unity.GraphToolkit.Utility.Editor`
- `Game1Graph.Runtime`
  - references: `GraphCore.Runtime`
- `Game1Graph.Editor`
  - not present in current tree

## 7. Compile Result

- has errors / not verified in Unity editor from CLI session.
- Static pass completed: target legacy/shim files removed and usages switched in code.

## 8. Conclusion

- GraphCore repo-ready: **yes** (clean base framework structure, no BusinessQuest* files)
- Game1Graph repo-ready: **yes** (only game-specific runtime layer retained)
- Remaining note: legacy `BusinessQuestGraphRunner` still exists in gameplay layer and remains intentionally untouched.
