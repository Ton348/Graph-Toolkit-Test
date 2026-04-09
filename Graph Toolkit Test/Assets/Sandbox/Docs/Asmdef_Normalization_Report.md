# Asmdef Normalization Report

## 1. Checked Actual Dependencies

### Legacy.Runtime
- Uses Game1Graph.Runtime: yes
- Files + types:
  - `Assets/Scripts/NPCManager.cs` -> `BusinessQuestGraph`
  - `Assets/Scripts/Tools/Graph/Core/Runtime/ConditionEvaluator.cs` -> `ConditionNode`, `ConditionType`, `PlayerStatType`

### Game1Graph.Runtime
- Uses Legacy.Runtime: no
- Files + types: none found

### Sandbox
- Uses GraphCore.Runtime: yes
- Uses Game1Graph.Runtime: yes
- Uses Legacy.Runtime: yes
- Files + types per dependency:
  - GraphCore.Runtime:
    - `Assets/Sandbox/BusinessQuestGraphRunner.cs` -> `GraphExecutionContext`, `GraphContextKey<T>` usage via `GraphContextKeys`
    - `Assets/Sandbox/GraphExecutionEvent.cs` -> `GraphExecutionContext`
    - `Assets/Sandbox/GraphCoreRuntimeApi.cs` -> `GraphExecutionContext`, `GraphContextKey`, `GraphContextKey<T>`
  - Game1Graph.Runtime:
    - `Assets/Sandbox/BusinessQuestGraphRunner.cs` -> `BusinessQuestGraph`, `BusinessQuestNode`, `ChoiceNode`, `ConditionNode`, `CheckpointNode`, `DialogueNode`, `StartNode`, `EndNode`, `GoToPointNode`, `SetGameObjectActiveNode`, `AddMapMarkerNode`, `RequestStartQuestNode`, `RequestCompleteQuestNode`, `RequestBuyBuildingNode`, `RequestHireBusinessWorkerNode`, `RequestTradeOfferNode`, `RequestOpenBusinessNode`, `RequestCloseBusinessNode`, `RequestSetBusinessOpenNode`, `RequestSetBusinessMarkupNode`, `RequestAssignBusinessTypeNode`, `RequestAssignSupplierNode`, `RequestInstallBusinessModuleNode`, `RequestUnlockContactNode`, `RefreshProfileNode`
    - `Assets/Sandbox/Services/IGameServer.cs` -> `QuestActionType`
    - `Assets/Sandbox/GraphCoreRuntimeApi.cs` -> `BusinessQuestGraph`, `BusinessQuestNode`, `BusinessQuestGraphRunner`
  - Legacy.Runtime:
    - `Assets/Sandbox/BusinessQuestGraphRunner.cs` -> `GameBootstrap`, `IGameServer`, `ProfileSyncService`, `RequestManager`, `GameDataRepository`, `PlayerStateSync`, `BusinessStateSyncService`, `DialogueService`, `ChoiceUIService`, `TradeOfferUIService`, `MapMarkerService`, `GraphProgressService`, `ServerActionResult`, `CompassManager`
    - `Assets/Sandbox/Services/*.cs` -> `ServerActionResult`, `ProfileSnapshot`, `GameRuntimeState`, `QuestState`, `QuestStatus`, `QuestDefinitionData`
    - `Assets/Sandbox/Compass/QuestCompassSync.cs` -> `GameDataRepository`, `PlayerStateSync`, `ProfileSnapshot`, `CompassManager`
    - `Assets/Sandbox/InteractionContext.cs` -> `NPCManager`

## 2. Changed asmdef

No asmdef references changed in this normalization pass.

Reason:
- `Legacy.Runtime -> Game1Graph.Runtime` is factually required.
- `Game1Graph.Runtime -> Legacy.Runtime` is not factually required.
- `Sandbox` references (`GraphCore.Runtime`, `Game1Graph.Runtime`, `Legacy.Runtime`) are all factually required.
- Editor asmdef files do not contain obvious removable references by current type usage.

## 3. Current Assembly Graph

- `GraphCore.Runtime` -> (no asmdef references)
- `GraphCore.Editor` -> `GraphCore.Runtime`, `Unity.GraphToolkit.Editor`, `Unity.GraphToolkit.Common.Editor`, `Unity.GraphToolkit.Utility.Editor`
- `Game1Graph.Runtime` -> `GraphCore.Runtime`
- `Game1Graph.Editor` -> `Game1Graph.Runtime`, `GraphCore.Editor`
- `Legacy.Runtime` -> `GraphCore.Runtime`, `Game1Graph.Runtime`
- `Legacy.Editor` -> `Legacy.Runtime`
- `Sandbox` -> `GraphCore.Runtime`, `Game1Graph.Runtime`, `Legacy.Runtime`

## 4. Cyclic Dependency Status

- Cyclic dependency: not resolved in this step.
- In the latest log sample, explicit cyclic chain text was not emitted, but compile state remains broken and historical cycle reports remain relevant.
- Most probable unresolved cycle source remains mixed legacy/predefined assemblies + cross-module runtime usage.

## 5. Compile Result

- Total compile errors (latest sampled from `Editor.log` tail): 331 unique error sites.
- Before/After for this step: unchanged (asmdef graph kept as-is).
- Top-10 files by error count (latest sampled):
  1. `Assets/GraphCore/Runtime/BusinessQuestGraphRunner.cs` (186)
  2. `Assets/Sandbox/Services/LocalGameServer.cs` (132)
  3. `Assets/Scripts/Business/UI/BusinessDetailsView.cs` (108)
  4. `Assets/GraphCore/Runtime/GraphCoreRuntimeApi.cs` (90)
  5. `Assets/Sandbox/Services/RemoteGameServer.cs` (84)
  6. `Assets/Sandbox/Services/IGameServer.cs` (75)
  7. `Assets/Game1Graph/Runtime/BusinessQuestGraphRunner.cs` (66)
  8. `Assets/GraphCore/Runtime/GraphExecutionEvent.cs` (48)
  9. `Assets/GraphCore/Editor/BusinessQuestGraphImporter.cs` (45)
  10. `Assets/Sandbox/BusinessQuestGraphRunner.cs` (33)

## 6. Conclusions

- `Legacy.Runtime -> Game1Graph.Runtime` is valid and should not be removed.
- `Game1Graph.Runtime -> Legacy.Runtime` is currently unnecessary and correctly absent.
- `Sandbox` legitimately depends on all three runtime assemblies.
- The biggest blockers are legacy files still compiling from old paths and mixed old/new graph layers.
- The current assembly issues are driven more by structural overlap than by one wrong asmdef reference.
