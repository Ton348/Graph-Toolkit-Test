# Graph Toolkit Audit Report

## 1. Current Graph Files

### Assets/Scripts/Tools/Graph (current structure)

- `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestEditorGraph.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestEditorNode.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestGraphImporter.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/ConditionNodeViewDecorator.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/GraphCoreEditorApi.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/BusinessQuestGraph.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/BusinessQuestGraphRunner.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/BusinessQuestNode.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/ConditionEvaluator.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/GraphContextKey.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/GraphContextKeys.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/GraphCoreRuntimeApi.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/GraphExecutionContext.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/GraphExecutionEvent.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/InteractionContext.cs`
- `Assets/Scripts/Tools/Graph/Core/Runtime/StealContextEvaluator.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/BusinessEditorApi.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/BusinessQuestBusinessNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckBusinessExistsNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckBusinessModuleInstalledNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckBusinessOpenNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckContactKnownNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestAssignBusinessTypeNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestAssignSupplierNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestBuyBuildingNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestCloseBusinessNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestHireBusinessWorkerNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestInstallBusinessModuleNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestOpenBusinessNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestRentBusinessNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestSetBusinessMarkupNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestSetBusinessOpenNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestTradeOfferNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestUnlockContactNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/BusinessRuntimeApi.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckBusinessExistsNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckBusinessModuleInstalledNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckBusinessOpenNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckContactKnownNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestAssignBusinessTypeNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestAssignSupplierNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestBuyBuildingNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestCloseBusinessNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestHireBusinessWorkerNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestInstallBusinessModuleNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestOpenBusinessNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestRentBusinessNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestSetBusinessMarkupNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestSetBusinessOpenNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestTradeOfferNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestUnlockContactNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/AddMapMarkerNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/BusinessQuestCommonNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/ChoiceNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/CommonEditorApi.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/ConditionNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/DialogueNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/EndNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/GoToPointNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/SetGameObjectActiveNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/StartNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/AddMapMarkerNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ChoiceNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ChoiceOption.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/CommonRuntimeApi.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ConditionNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ConditionType.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/DialogueNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/EndNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/GoToPointNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/PlayerStatType.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/SetGameObjectActiveNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/SkillType.cs`
- `Assets/Scripts/Tools/Graph/Features/Common/Runtime/StartNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/BusinessQuestQuestNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/CheckpointNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/QuestEditorApi.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/RefreshProfileNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/RequestCompleteQuestNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/RequestStartQuestNodeModel.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/CheckpointNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/QuestActionType.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/QuestRuntimeApi.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/RefreshProfileNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/RequestCompleteQuestNode.cs`
- `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/RequestStartQuestNode.cs`

### Integration files outside Tools/Graph

- `Assets/Scripts/Services/GraphProgressService.cs`
- `Assets/Scripts/Services/QuestService.cs`
- `Assets/Scripts/Services/PlayerStateSync.cs`
- `Assets/Scripts/Services/IGameServer.cs`
- `Assets/Scripts/Services/LocalGameServer.cs`
- `Assets/Scripts/Services/RemoteGameServer.cs`
- `Assets/Scripts/UI/DialogueService.cs`
- `Assets/Scripts/UI/ChoiceUIService.cs`
- `Assets/Scripts/UI/QuestListUI.cs`
- `Assets/Scripts/Compass/QuestCompassSync.cs`
- `server/index.js`
- `server/data/quests.json`
- `server/playerData/player.json`

---

## 2. Classification

| File | Path | Zone | Reason |
|------|------|------|--------|
| BusinessQuestEditorGraph.cs | `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestEditorGraph.cs` | GraphCore.Editor | Core graph editor asset/model/import/decorator infrastructure. |
| BusinessQuestEditorNode.cs | `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestEditorNode.cs` | GraphCore.Editor | Core graph editor asset/model/import/decorator infrastructure. |
| BusinessQuestGraphImporter.cs | `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestGraphImporter.cs` | GraphCore.Editor | Core graph editor asset/model/import/decorator infrastructure. |
| ConditionNodeViewDecorator.cs | `Assets/Scripts/Tools/Graph/Core/Editor/ConditionNodeViewDecorator.cs` | GraphCore.Editor | Core graph editor asset/model/import/decorator infrastructure. |
| GraphCoreEditorApi.cs | `Assets/Scripts/Tools/Graph/Core/Editor/GraphCoreEditorApi.cs` | GraphCore.Editor | Core graph editor asset/model/import/decorator infrastructure. |
| BusinessQuestGraph.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/BusinessQuestGraph.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| BusinessQuestGraphRunner.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/BusinessQuestGraphRunner.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| BusinessQuestNode.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/BusinessQuestNode.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| ConditionEvaluator.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/ConditionEvaluator.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| GraphContextKey.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/GraphContextKey.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| GraphContextKeys.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/GraphContextKeys.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| GraphCoreRuntimeApi.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/GraphCoreRuntimeApi.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| GraphExecutionContext.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/GraphExecutionContext.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| GraphExecutionEvent.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/GraphExecutionEvent.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| InteractionContext.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/InteractionContext.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| StealContextEvaluator.cs | `Assets/Scripts/Tools/Graph/Core/Runtime/StealContextEvaluator.cs` | GraphCore.Runtime | Core graph runtime model/execution/context/event infrastructure. |
| BusinessEditorApi.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/BusinessEditorApi.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| BusinessQuestBusinessNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/BusinessQuestBusinessNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CheckBusinessExistsNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckBusinessExistsNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CheckBusinessModuleInstalledNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckBusinessModuleInstalledNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CheckBusinessOpenNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckBusinessOpenNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CheckContactKnownNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/CheckContactKnownNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestAssignBusinessTypeNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestAssignBusinessTypeNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestAssignSupplierNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestAssignSupplierNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestBuyBuildingNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestBuyBuildingNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestCloseBusinessNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestCloseBusinessNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestHireBusinessWorkerNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestHireBusinessWorkerNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestInstallBusinessModuleNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestInstallBusinessModuleNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestOpenBusinessNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestOpenBusinessNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestRentBusinessNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestRentBusinessNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestSetBusinessMarkupNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestSetBusinessMarkupNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestSetBusinessOpenNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestSetBusinessOpenNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestTradeOfferNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestTradeOfferNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestUnlockContactNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Business/Editor/RequestUnlockContactNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| BusinessRuntimeApi.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/BusinessRuntimeApi.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| CheckBusinessExistsNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckBusinessExistsNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| CheckBusinessModuleInstalledNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckBusinessModuleInstalledNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| CheckBusinessOpenNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckBusinessOpenNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| CheckContactKnownNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/CheckContactKnownNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestAssignBusinessTypeNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestAssignBusinessTypeNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestAssignSupplierNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestAssignSupplierNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestBuyBuildingNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestBuyBuildingNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestCloseBusinessNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestCloseBusinessNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestHireBusinessWorkerNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestHireBusinessWorkerNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestInstallBusinessModuleNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestInstallBusinessModuleNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestOpenBusinessNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestOpenBusinessNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestRentBusinessNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestRentBusinessNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestSetBusinessMarkupNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestSetBusinessMarkupNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestSetBusinessOpenNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestSetBusinessOpenNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestTradeOfferNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestTradeOfferNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestUnlockContactNode.cs | `Assets/Scripts/Tools/Graph/Features/Business/Runtime/RequestUnlockContactNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| AddMapMarkerNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/AddMapMarkerNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| BusinessQuestCommonNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/BusinessQuestCommonNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| ChoiceNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/ChoiceNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CommonEditorApi.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/CommonEditorApi.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| ConditionNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/ConditionNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| DialogueNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/DialogueNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| EndNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/EndNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| GoToPointNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/GoToPointNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| SetGameObjectActiveNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/SetGameObjectActiveNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| StartNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Common/Editor/StartNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| AddMapMarkerNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/AddMapMarkerNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| ChoiceNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ChoiceNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| ChoiceOption.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ChoiceOption.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| CommonRuntimeApi.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/CommonRuntimeApi.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| ConditionNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ConditionNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| ConditionType.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/ConditionType.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| DialogueNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/DialogueNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| EndNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/EndNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| GoToPointNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/GoToPointNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| PlayerStatType.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/PlayerStatType.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| SetGameObjectActiveNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/SetGameObjectActiveNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| SkillType.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/SkillType.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| StartNode.cs | `Assets/Scripts/Tools/Graph/Features/Common/Runtime/StartNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| BusinessQuestQuestNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Editor/BusinessQuestQuestNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CheckpointNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Editor/CheckpointNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| QuestEditorApi.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Editor/QuestEditorApi.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RefreshProfileNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Editor/RefreshProfileNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestCompleteQuestNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Editor/RequestCompleteQuestNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| RequestStartQuestNodeModel.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Editor/RequestStartQuestNodeModel.cs` | GameSpecific | Game feature editor node models for graph authoring. |
| CheckpointNode.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/CheckpointNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| QuestActionType.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/QuestActionType.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| QuestRuntimeApi.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/QuestRuntimeApi.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RefreshProfileNode.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/RefreshProfileNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestCompleteQuestNode.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/RequestCompleteQuestNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| RequestStartQuestNode.cs | `Assets/Scripts/Tools/Graph/Features/Quest/Runtime/RequestStartQuestNode.cs` | GameSpecific | Game feature runtime nodes/types executed by graph runner. |
| GraphProgressService.cs | `Assets/Scripts/Services/GraphProgressService.cs` | GraphCore.Server | Persists graph progress/checkpoints and sync boundary to backend. |
| QuestService.cs | `Assets/Scripts/Services/QuestService.cs` | GraphCore.Server | Quest API boundary used by graph runtime actions. |
| PlayerStateSync.cs | `Assets/Scripts/Services/PlayerStateSync.cs` | GraphCore.Server | Runtime/player checkpoint sync used by graph runner. |
| IGameServer.cs | `Assets/Scripts/Services/IGameServer.cs` | GraphCore.Server | Server contract consumed by graph runtime request nodes. |
| LocalGameServer.cs | `Assets/Scripts/Services/LocalGameServer.cs` | GraphCore.Server | Local server implementation for graph-related requests. |
| RemoteGameServer.cs | `Assets/Scripts/Services/RemoteGameServer.cs` | GraphCore.Server | Remote server implementation for graph-related requests. |
| DialogueService.cs | `Assets/Scripts/UI/DialogueService.cs` | GameSpecific | Dialogue presentation endpoint used by dialogue graph node. |
| ChoiceUIService.cs | `Assets/Scripts/UI/ChoiceUIService.cs` | GameSpecific | Choice presentation endpoint used by choice graph node. |
| QuestListUI.cs | `Assets/Scripts/UI/QuestListUI.cs` | GameSpecific | Quest UI integration potentially triggered by graph flow. |
| QuestCompassSync.cs | `Assets/Scripts/Compass/QuestCompassSync.cs` | GameSpecific | Quest tracking/compass bridge influenced by quest state. |
| index.js | `server/index.js` | GraphCore.Server | Main backend entrypoint serving quest/business APIs used by graph. |
| quests.json | `server/data/quests.json` | GraphCore.Server | Server quest dataset consumed by graph-driven requests. |
| player.json | `server/playerData/player.json` | GraphCore.Server | Server-side persisted player graph checkpoints/state. |

---

## 3. Ambiguous Files

- `Assets/Scripts/Tools/Graph/Core/Editor/GraphCoreEditorApi.cs` — bridge aliases (`GraphEditorGraph`, `GraphEditorNode`, `GraphImporter`), может быть transition layer (GraphCore.Editor или Legacy bridge).
- `Assets/Scripts/Tools/Graph/Features/Common/Editor/CommonEditorApi.cs` — API-wrapper слой, не основной authoring-слой; ближе к transition/compat.
- `Assets/Scripts/Tools/Graph/Features/Quest/Editor/QuestEditorApi.cs` — API-wrapper слой, вероятно bridge для внешнего API/неймспейсов.
- `Assets/Scripts/Tools/Graph/Features/Business/Editor/BusinessEditorApi.cs` — API-wrapper слой, вероятно bridge для внешнего API/неймспейсов.
- `Assets/Scripts/Tools/Graph/Core/Editor/ConditionNodeViewDecorator.cs` — editor-only UX слой; функционально не core runtime, но тесно связан с авторингом.

---

## 4. Legacy / Transition Layer

- Legacy naming:
  - `BusinessQuestEditorGraph`, `BusinessQuestEditorNode`, `BusinessQuestGraphImporter`
  - `BusinessQuestGraph`, `BusinessQuestGraphRunner`, `BusinessQuestNode`
  - `BusinessQuestBusinessNodeModel`, `BusinessQuestCommonNodeModel`, `BusinessQuestQuestNodeModel`
- Transition/bridge API classes:
  - `GraphCoreEditorApi.cs` (`GraphEditorGraph`, `GraphEditorNode`, `GraphImporter`)
  - `GraphCoreRuntimeApi.cs`
  - `BusinessEditorApi.cs`, `CommonEditorApi.cs`, `QuestEditorApi.cs`
  - `BusinessRuntimeApi.cs`, `CommonRuntimeApi.cs`, `QuestRuntimeApi.cs`
- Candidate rename/move (analysis only):
  - All `BusinessQuest*` core classes -> neutral GraphCore naming.
  - API-wrapper files (`*EditorApi.cs`, `*RuntimeApi.cs`) as explicit compat layer folder.
  - `ConditionNodeViewDecorator` as optional editor extension layer.

---

## 5. Preliminary Split Proposal

### GraphCore.Editor

- `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestEditorGraph.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestEditorNode.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/BusinessQuestGraphImporter.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/ConditionNodeViewDecorator.cs`
- `Assets/Scripts/Tools/Graph/Core/Editor/GraphCoreEditorApi.cs` (bridge/compat)

### GraphCore.Runtime

- `Assets/Scripts/Tools/Graph/Core/Runtime/*` (all core runtime files)
  - Graph model/execution/context/event/evaluator/runner.

### GraphCore.Server

- `Assets/Scripts/Services/IGameServer.cs`
- `Assets/Scripts/Services/LocalGameServer.cs`
- `Assets/Scripts/Services/RemoteGameServer.cs`
- `Assets/Scripts/Services/GraphProgressService.cs`
- `Assets/Scripts/Services/PlayerStateSync.cs`
- `Assets/Scripts/Services/QuestService.cs`
- `server/index.js`
- `server/data/quests.json`
- `server/playerData/player.json`

### Not for Core

- `Assets/Scripts/Tools/Graph/Features/*` (Business/Common/Quest nodes: game-domain specific)
- `Assets/Scripts/UI/DialogueService.cs`
- `Assets/Scripts/UI/ChoiceUIService.cs`
- `Assets/Scripts/UI/QuestListUI.cs`
- `Assets/Scripts/Compass/QuestCompassSync.cs`
