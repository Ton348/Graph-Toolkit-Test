# BaseNodes Execution Integration Report

## importer changes
- Updated `Assets/Game1Graph/Editor/BusinessQuestGraphImporter.cs`.
- Added model -> runtime mapping for new Wave1/Wave2 models:
  - `GraphCore.BaseNodes.Editor.Flow.StartNodeModel` -> `GraphCore.BaseNodes.Runtime.Flow.StartNode`
  - `GraphCore.BaseNodes.Editor.Flow.FinishNodeModel` -> `GraphCore.BaseNodes.Runtime.Flow.FinishNode`
  - `GraphCore.BaseNodes.Editor.UI.DialogueNodeModel` -> `GraphCore.BaseNodes.Runtime.UI.DialogueNode`
  - `GraphCore.BaseNodes.Editor.UI.ChoiceNodeModel` -> `GraphCore.BaseNodes.Runtime.UI.ChoiceNode`
  - `GraphCore.BaseNodes.Editor.Flow.DelayNodeModel` -> `GraphCore.BaseNodes.Runtime.Flow.DelayNode`
  - `GraphCore.BaseNodes.Editor.Flow.RandomNodeModel` -> `GraphCore.BaseNodes.Runtime.Flow.RandomNode`
  - `GraphCore.BaseNodes.Editor.Utility.LogNodeModel` -> `GraphCore.BaseNodes.Runtime.Utility.LogNode`
- Added connection mapping for:
  - Base `ChoiceNode` options `Option1..Option4` -> runtime option `nextNodeId`
  - Base `RandomNode` outputs `Option1..Option4` -> runtime option `nextNodeId`
- Added option mapping:
  - Choice labels `Option1..Option4` -> runtime `ChoiceOption.label`
  - Random weights `Weight1..Weight4` -> runtime `RandomOption.weight`
- Implementation uses reflection for runtime type creation to avoid coupling importer to legacy type names and to keep mapping isolated.

## runtime execution changes
- Updated `Assets/Sandbox/BusinessQuestGraphRunner.cs` with isolated handling branch for `GraphCore.BaseNodes.*` types:
  - `StartNode`: pass-through to `nextNodeId`
  - `FinishNode`: clean stop (`Stop()`)
  - `DialogueNode`: uses existing `DialogueService` path, then continues
  - `ChoiceNode`: uses existing `ChoiceUIService` path, then branches by selected option
  - `LogNode`: `Debug.Log(message)` and continue
  - `DelayNode`: waits `delaySeconds` via tick-driven timer, then continue
  - `RandomNode`: weighted branch selection and continue
- Legacy business/prototype cases were not removed/rewired; base-node handling is additive.

## какие сервисы использованы
- `DialogueService`
- `ChoiceUIService`
- `UnityEngine.Debug`
- `UnityEngine.Time`
- `UnityEngine.Random`

## какие ноды реально исполняются end-to-end
- With current integration code path: `StartNode`, `FinishNode`, `DialogueNode`, `ChoiceNode`, `LogNode`, `DelayNode`, `RandomNode`.
- Mapping + execution path for these nodes is present end-to-end (import + runtime branch).

## что еще осталось для полной clean pipeline
- Current project assembly issues still produce compile noise from legacy/sandbox dependencies, which can block runtime verification in editor.
- For fully clean pipeline, next step should extract a dedicated clean runner path in GraphCore runtime (separate from sandbox runner).
- Editor errors shown in `Editor.log` for Wave files look stale/historical and need a clean full recompile validation pass.
