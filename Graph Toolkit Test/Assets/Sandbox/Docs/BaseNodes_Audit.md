# Base Nodes Audit

## 1. Summary
- total target nodes: 13
- fully reusable: 3
- require rename/refactor: 6
- missing: 4
- prototype-contaminated: 6

## 2. Node-by-Node Audit

### StartNode
- Spec: entry node, single execution output.
- Current editor file: `Assets/Game1Graph/Editor/Common/StartNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Common/StartNode.cs`
- Current location: `Assets/Game1Graph/Editor/Common`, `Assets/Game1Graph/Runtime/Common`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: match
  - Fields: minimal (no extra fields)
  - Ports: output only (editor), runtime uses `nextNodeId`
  - Behavior: runner goes to `nextNodeId`
  - Client/Server: Client
- PrototypeContaminated: no
- Status: OK
- Recommendation: Reuse as base

### FinishNode
- Spec: graph termination node.
- Current editor file: `Assets/Game1Graph/Editor/Common/EndNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Common/EndNode.cs`
- Current location: `Assets/Game1Graph/Editor/Common`, `Assets/Game1Graph/Runtime/Common`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: mismatch (`EndNode` vs `FinishNode`)
  - Fields: extra `clearCheckpoint`, `completeQuestId`
  - Ports: input only
  - Behavior: stop execution; optionally server `TryCompleteQuestAsync` and checkpoint clear
  - Client/Server: Mixed (Client + optional Server)
- PrototypeContaminated: yes
- Status: Rename
- Recommendation: Rework existing

### DialogueNode
- Spec: show dialogue text/media and continue.
- Current editor file: `Assets/Game1Graph/Editor/Common/DialogueNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Common/DialogueNode.cs`
- Current location: `Assets/Game1Graph/Editor/Common`, `Assets/Game1Graph/Runtime/Common`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: match
  - Fields: `title`, `bodyText`, `screenshot`
  - Ports: input + output
  - Behavior: `DialogueService.ShowDialogue(...)`, then next
  - Client/Server: Client
- PrototypeContaminated: no
- Status: OK
- Recommendation: Reuse as base

### ChoiceNode
- Spec: branch selection node.
- Current editor file: `Assets/Game1Graph/Editor/Common/ChoiceNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Common/ChoiceNode.cs`
- Current location: `Assets/Game1Graph/Editor/Common`, `Assets/Game1Graph/Runtime/Common`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: match
  - Fields: runtime list `options`; editor hardcoded 4 labels/ports
  - Ports: input + 4 fixed outputs in editor
  - Behavior: `ChoiceUIService` callback -> selected branch
  - Client/Server: Client
- PrototypeContaminated: no
- Status: Refactor
- Recommendation: Rework existing

### PlayCutsceneNode
- Spec: play timeline/cutscene and continue.
- Current editor file: missing
- Current runtime file: missing
- Current location: n/a
- Current assembly: n/a
- Match:
  - Name: missing
  - Fields: missing
  - Ports: missing
  - Behavior: missing
  - Client/Server: unknown
- PrototypeContaminated: no
- Status: CreateFromScratch
- Recommendation: Rebuild clean

### DelayNode
- Spec: wait for duration and continue.
- Current editor file: missing
- Current runtime file: missing
- Current location: n/a
- Current assembly: n/a
- Match:
  - Name: missing
  - Fields: missing
  - Ports: missing
  - Behavior: missing
  - Client/Server: Client (expected)
- PrototypeContaminated: no
- Status: CreateFromScratch
- Recommendation: Rebuild clean

### RandomNode
- Spec: random branch selection.
- Current editor file: missing
- Current runtime file: missing
- Current location: n/a
- Current assembly: n/a
- Match:
  - Name: missing
  - Fields: missing
  - Ports: missing
  - Behavior: missing
  - Client/Server: Client (expected)
- PrototypeContaminated: no
- Status: CreateFromScratch
- Recommendation: Rebuild clean

### LogNode
- Spec: debug/log output node.
- Current editor file: missing
- Current runtime file: missing
- Current location: n/a
- Current assembly: n/a
- Match:
  - Name: missing
  - Fields: missing
  - Ports: missing
  - Behavior: missing
  - Client/Server: Client (expected)
- PrototypeContaminated: no
- Status: CreateFromScratch
- Recommendation: Rebuild clean

### MapMarkerNode
- Spec: place/show map marker and continue.
- Current editor file: `Assets/Game1Graph/Editor/Common/AddMapMarkerNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Common/AddMapMarkerNode.cs`
- Current location: `Assets/Game1Graph/Editor/Common`, `Assets/Game1Graph/Runtime/Common`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: mismatch (`AddMapMarkerNode` vs `MapMarkerNode`)
  - Fields: `markerId`, `targetTransform`, `title`
  - Ports: input + output
  - Behavior: `MapMarkerService.ShowMarker` + `CompassManager.ShowTarget`
  - Client/Server: Client
- PrototypeContaminated: yes
- Status: Rename
- Recommendation: Rework existing

### CheckpointNode
- Spec: save/restore graph checkpoint.
- Current editor file: `Assets/Game1Graph/Editor/Quest/CheckpointNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Quest/CheckpointNode.cs`
- Current location: `Assets/Game1Graph/Editor/Quest`, `Assets/Game1Graph/Runtime/Quest`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: match
  - Fields: `checkpointId`
  - Ports: input + output
  - Behavior: save via `IGameServer.TrySaveCheckpointAsync`, resume via `PlayerStateSync`
  - Client/Server: Mixed (Client + Server)
- PrototypeContaminated: yes
- Status: Refactor
- Recommendation: Rework existing

### StartQuestNode
- Spec: request quest start with success/fail branching.
- Current editor file: `Assets/Game1Graph/Editor/Quest/RequestStartQuestNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Quest/RequestStartQuestNode.cs`
- Current location: `Assets/Game1Graph/Editor/Quest`, `Assets/Game1Graph/Runtime/Quest`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: mismatch (`RequestStartQuestNode` vs `StartQuestNode`)
  - Fields: `questId`, `successNodeId`, `failNodeId`
  - Ports: input + success/fail
  - Behavior: `IGameServer.TryStartQuestAsync`
  - Client/Server: Server
- PrototypeContaminated: yes
- Status: Rename
- Recommendation: Rework existing

### CompleteQuestNode
- Spec: request quest completion with success/fail branching.
- Current editor file: `Assets/Game1Graph/Editor/Quest/RequestCompleteQuestNodeModel.cs`
- Current runtime file: `Assets/Game1Graph/Runtime/Quest/RequestCompleteQuestNode.cs`
- Current location: `Assets/Game1Graph/Editor/Quest`, `Assets/Game1Graph/Runtime/Quest`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`
- Match:
  - Name: mismatch (`RequestCompleteQuestNode` vs `CompleteQuestNode`)
  - Fields: `questId`, `successNodeId`, `failNodeId`
  - Ports: input + success/fail
  - Behavior: `IGameServer.TryCompleteQuestAsync`
  - Client/Server: Server
- PrototypeContaminated: yes
- Status: Rename
- Recommendation: Rework existing

### QuestStateConditionNode
- Spec: branch by quest state.
- Current editor file: partial via `Assets/Game1Graph/Editor/Common/ConditionNodeModel.cs`
- Current runtime file: partial via `Assets/Game1Graph/Runtime/Common/ConditionNode.cs` + `Assets/Scripts/Tools/Graph/Core/Runtime/ConditionEvaluator.cs`
- Current location: `Game1Graph/Common` + legacy evaluator in `Assets/Scripts/Tools/Graph/Core/Runtime`
- Current assembly: `Game1Graph.Editor`, `Game1Graph.Runtime`, `Legacy.Runtime`
- Match:
  - Name: mismatch (generic `ConditionNode`)
  - Fields: generic condition payload, includes `questId`
  - Ports: input + True/False
  - Behavior: quest-state checks embedded in generic evaluator (`QuestActive`, `QuestCompleted`)
  - Client/Server: Client
- PrototypeContaminated: yes
- Status: Refactor
- Recommendation: Rebuild clean

## 3. Candidate Base Set
- `StartNode`
- `DialogueNode`
- `ChoiceNode` (после минимальной правки портов/структуры options)
- `CheckpointNode` (после отделения инфраструктурной части)
- `MapMarkerNode` (после переименования и отвязки от prototype service API)

## 4. Nodes To Rebuild
- `PlayCutsceneNode`
- `DelayNode`
- `RandomNode`
- `LogNode`
- `QuestStateConditionNode`

## 5. Prototype-Only Contamination
Ноды/файлы с прямой завязкой на prototype business/sandbox слой:
- `Assets/Sandbox/BusinessQuestGraphRunner.cs`
- `EndNode` / `EndNodeModel` (`completeQuestId`, checkpoint clear + server call)
- `AddMapMarkerNode` / `AddMapMarkerNodeModel` (через `MapMarkerService`, `CompassManager`)
- `CheckpointNode` / `CheckpointNodeModel` (через `IGameServer`, `PlayerStateSync`, `GraphProgressService`)
- `RequestStartQuestNode` / `RequestStartQuestNodeModel` (через `IGameServer`)
- `RequestCompleteQuestNode` / `RequestCompleteQuestNodeModel` (через `IGameServer`)
- `ConditionNode` + `ConditionEvaluator` (зависимость от `PlayerStateSync` в legacy runtime)

## 6. Recommended Next Implementation Order
1. `StartNode` + `FinishNode` (clean version)
2. `DialogueNode`
3. `ChoiceNode`
4. `LogNode`
5. `DelayNode`
6. `RandomNode`
7. `MapMarkerNode`
8. `QuestStateConditionNode`
9. `CheckpointNode`
10. `StartQuestNode`
11. `CompleteQuestNode`
12. `PlayCutsceneNode`
