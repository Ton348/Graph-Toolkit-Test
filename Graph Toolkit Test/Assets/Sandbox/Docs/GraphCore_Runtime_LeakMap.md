# GraphCore Runtime Leak Map

## 1. Compile Errors

### A) Зависимость на игровые ноды (Game1Graph.Runtime)
- `Assets/GraphCore/Runtime/BusinessQuestGraphRunner.cs`: не найдены `StartNode`, `EndNode`, `DialogueNode`, `ChoiceNode`, `ConditionNode`, `CheckpointNode`, `GoToPointNode`, `SetGameObjectActiveNode`, `AddMapMarkerNode`, `RefreshProfileNode`, `Request*` и `Check*` node-типы, `ChoiceOption`.
- `Assets/GraphCore/Runtime/BusinessQuestGraph.cs`: не найден `CheckpointNode`.

### B) Зависимость на UI/services (Sandbox + legacy Services/UI)
- `Assets/GraphCore/Runtime/BusinessQuestGraphRunner.cs`: не найдены `IGameServer`, `PlayerStateSync`, `DialogueService`, `ChoiceUIService`, `GraphProgressService`, `TradeOfferUIService`, `MapMarkerService`, `ServerActionResult`.
- `Assets/GraphCore/Runtime/GraphCoreRuntimeApi.cs`: не найдены `global::IGameServer`, `global::DialogueService`, `global::ChoiceUIService`, `global::TradeOfferUIService`, `global::MapMarkerService`, `global::GraphProgressService`, `global::ServerActionResult`.
- `Assets/GraphCore/Runtime/GraphExecutionEvent.cs`: не найден `ServerActionResult`.
- `Assets/GraphCore/Runtime/GraphContextKeys.cs`: не найден `ServerActionResult`.

### C) Зависимость на старый bootstrap/infrastructure (old Assets/Scripts)
- `Assets/GraphCore/Runtime/BusinessQuestGraphRunner.cs`: не найдены `GameBootstrap`, `ProfileSyncService`, `RequestManager`, `GameDataRepository`, `BusinessStateSyncService`, `CompassManager`.
- `Assets/GraphCore/Runtime/GraphCoreRuntimeApi.cs`: не найден `global::GameBootstrap`.
- `Assets/GraphCore/Runtime/InteractionContext.cs`: не найден `NPCManager`.

### D) Зависимость на legacy wrapper/API слой
- `GraphCoreRuntimeApi.cs` использует `global::`-типы из внешних слоев; при разделении asmdef это стало жесткой протечкой в Core.

## 2. File Dependency Map

### BusinessQuestGraphRunner.cs
- External types used:
  - `GameBootstrap` -> `Assets/Scripts/Bootstrap/GameBootstrap.cs` -> Should move to Sandbox
  - `IGameServer` -> `Assets/Sandbox/Services/IGameServer.cs` -> Should be replaced by interface/abstraction later
  - `ProfileSyncService` -> `Assets/Scripts/Services/ProfileSyncService.cs` -> Should move to Sandbox
  - `RequestManager` -> `Assets/Scripts/Services/RequestManager.cs` -> Should move to Sandbox
  - `GameDataRepository` -> `Assets/Scripts/GameData/GameDataRepository.cs` -> Should move to Sandbox
  - `PlayerStateSync` -> `Assets/Sandbox/Services/PlayerStateSync.cs` -> Should move to Sandbox
  - `BusinessStateSyncService` -> `Assets/Scripts/Business/Runtime/BusinessStateSyncService.cs` -> Should move to Sandbox
  - `DialogueService` -> `Assets/Sandbox/UI/DialogueService.cs` -> Should move to Sandbox
  - `ChoiceUIService` -> `Assets/Sandbox/UI/ChoiceUIService.cs` -> Should move to Sandbox
  - `TradeOfferUIService` -> `Assets/Scripts/UI/TradeOfferUIService.cs` -> Should move to Sandbox
  - `MapMarkerService` -> `Assets/Scripts/Services/MapMarkerService.cs` -> Should move to Sandbox
  - `GraphProgressService` -> `Assets/Sandbox/Services/GraphProgressService.cs` -> Should move to Sandbox
  - `ServerActionResult` -> `Assets/Scripts/Services/ServerActionResult.cs` -> Should move to Sandbox
  - `CompassManager` -> `Assets/Scripts/Compass/CompassManager.cs` -> Should move to Sandbox
  - `ConditionEvaluator` -> `Assets/Scripts/Tools/Graph/Core/Runtime/ConditionEvaluator.cs` -> Legacy/unclear
  - `StartNode`/`EndNode`/`DialogueNode`/`ChoiceNode`/`ConditionNode`/`CheckpointNode`/`GoToPointNode`/`SetGameObjectActiveNode`/`AddMapMarkerNode`/`RefreshProfileNode`/`Request*`/`Check*`/`ChoiceOption` -> `Assets/Game1Graph/Runtime/*` -> Should move to Game1Graph
  - `UnityEngine` types (`Transform`, `Vector3`, `Mathf`, `Debug`) -> Unity/framework/third-party
- Verdict:
  - Must be moved out of GraphCore.Runtime

### GraphCoreRuntimeApi.cs
- External types used:
  - `global::BusinessQuestGraph`, `global::BusinessQuestNode`, `global::BusinessQuestGraphRunner`, `global::GraphExecutionContext`, `global::InteractionContext`, `global::GraphContextKey`, `global::GraphContextKeys` -> GraphCore.Runtime
  - `global::GameBootstrap` -> `Assets/Scripts/Bootstrap/GameBootstrap.cs` -> Should move to Sandbox
  - `global::IGameServer`, `global::DialogueService`, `global::ChoiceUIService`, `global::TradeOfferUIService`, `global::MapMarkerService`, `global::GraphProgressService`, `global::ServerActionResult` -> Sandbox/legacy services/ui -> Should move to Sandbox
  - `UnityEngine.Transform` -> Unity/framework/third-party
- Verdict:
  - Can stay only after introducing interfaces

### GraphExecutionEvent.cs
- External types used:
  - `ServerActionResult` -> `Assets/Scripts/Services/ServerActionResult.cs` -> Should move to Sandbox
  - `GraphExecutionContext`, `GraphContextKeys` -> GraphCore.Runtime
  - `System` -> Unity/framework/third-party
- Verdict:
  - Can stay only after introducing interfaces

### GraphContextKeys.cs
- External types used:
  - `GraphContextKey<T>` -> GraphCore.Runtime
  - `ServerActionResult` -> `Assets/Scripts/Services/ServerActionResult.cs` -> Should move to Sandbox
- Verdict:
  - Can stay only after introducing interfaces

### InteractionContext.cs
- External types used:
  - `NPCManager` -> `Assets/Scripts/NPCManager.cs` -> Should move to Sandbox
- Verdict:
  - Must be moved out of GraphCore.Runtime

### BusinessQuestGraph.cs
- External types used:
  - `CheckpointNode` -> `Assets/Game1Graph/Runtime/Quest/CheckpointNode.cs` -> Should move to Game1Graph
  - `BusinessQuestNode` -> GraphCore.Runtime
  - `UnityEngine.ScriptableObject`, `SerializeReference`, `List<T>` -> Unity/framework/third-party
- Verdict:
  - Can stay only after introducing interfaces

### BusinessQuestNode.cs
- External types used:
  - `System.Serializable` -> Unity/framework/third-party
- Verdict:
  - Can stay in GraphCore.Runtime as-is

### GraphExecutionContext.cs
- External types used:
  - `GraphContextKey<T>` -> GraphCore.Runtime
  - `System.Collections.Generic.Dictionary` -> Unity/framework/third-party
- Verdict:
  - Can stay in GraphCore.Runtime as-is

## 3. Dependency Groups

### Depends on Game1Graph.Runtime
- `StartNode`, `EndNode`, `DialogueNode`, `ChoiceNode`, `ConditionNode`, `CheckpointNode`, `GoToPointNode`, `SetGameObjectActiveNode`, `AddMapMarkerNode`, `RefreshProfileNode`
- `RequestBuyBuildingNode`, `RequestTradeOfferNode`, `RequestStartQuestNode`, `RequestCompleteQuestNode`
- `RequestRentBusinessNode`, `RequestAssignBusinessTypeNode`, `RequestInstallBusinessModuleNode`, `RequestAssignSupplierNode`, `RequestHireBusinessWorkerNode`, `RequestSetBusinessOpenNode`, `RequestOpenBusinessNode`, `RequestCloseBusinessNode`, `RequestSetBusinessMarkupNode`, `RequestUnlockContactNode`
- `CheckBusinessExistsNode`, `CheckBusinessOpenNode`, `CheckBusinessModuleInstalledNode`, `CheckContactKnownNode`
- `ChoiceOption`

### Depends on Sandbox
- `IGameServer`, `PlayerStateSync`, `DialogueService`, `ChoiceUIService`, `GraphProgressService`

### Depends on old Assets/Scripts
- `GameBootstrap`, `ProfileSyncService`, `RequestManager`, `GameDataRepository`, `BusinessStateSyncService`, `TradeOfferUIService`, `MapMarkerService`, `ServerActionResult`, `NPCManager`, `CompassManager`, `ConditionEvaluator`

### Depends on Unity / allowed external
- `UnityEngine` (`ScriptableObject`, `Transform`, `Debug`, `Vector3`, `Mathf`, `SerializeReference`)
- `System`, `System.Collections.Generic`, `System.Threading.Tasks`

## 4. Preliminary Recommendations

### Should remain in GraphCore.Runtime
- `BusinessQuestNode.cs`
- `GraphExecutionContext.cs`
- `GraphContextKey.cs`

### Should move to Game1Graph.Runtime
- `BusinessQuestGraphRunner.cs` (game-node orchestration)
- `BusinessQuestGraph.cs` (из-за `CheckpointNode` в текущем виде)

### Should move to Sandbox
- `InteractionContext.cs` (из-за `NPCManager`)

### Should be abstracted behind interfaces
- `ServerActionResult` usage in `GraphExecutionEvent.cs`, `GraphContextKeys.cs`, `GraphCoreRuntimeApi.cs`
- Constructor dependencies in `GraphCoreRuntimeApi.GraphRunner` (`IGameServer`, UI/services, bootstrap)
- Checkpoint lookup in `BusinessQuestGraph.cs` (через core-safe checkpoint contract)

### Ambiguous
- `GraphCoreRuntimeApi.cs` (по смыслу bridge-слой; может жить вне Core)
- `ConditionEvaluator` dependency path (файл остался в legacy core path вне `GraphCore.Runtime` asmdef)
