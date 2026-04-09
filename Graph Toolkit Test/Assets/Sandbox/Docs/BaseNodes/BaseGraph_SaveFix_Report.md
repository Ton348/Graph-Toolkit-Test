# BaseGraph Save Fix Report

## Changed Files
- Assets/GraphCore/Editor/BaseGraphAssetImporter.cs
- Assets/GraphCore/Editor/BaseGraphEditorGraph.cs
- Assets/GraphCore/Editor/BaseGraphImporter.cs
- Assets/GraphCore/Runtime/BaseGraph.cs
- Assets/GraphCore/Editor/BaseNodes/Flow/StartNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/Flow/FinishNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/Flow/DelayNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/Flow/RandomNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/UI/DialogueNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/UI/ChoiceNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/Utility/LogNodeModel.cs

## Asset Pipeline
- BaseGraphEditorGraph
  - dedicated graph type
  - `AssetExtension = "basegraph"`
  - separate create menu (`Assets/Create/Base Graph`)
- BaseGraphAssetImporter
  - dedicated `ScriptedImporter` for `.basegraph`
  - loads `BaseGraphEditorGraph`
  - builds runtime graph via `BaseGraphImporter.BuildBaseGraph(...)`
  - adds both assets:
    - `Graph` (main object when available)
    - `RuntimeGraph` (sub-asset)
- BaseGraphImporter
  - input: `BaseGraphEditorGraph`
  - only base node models are converted
  - fills `startNodeId`, `nodes`, `nextNodeId`, `ChoiceOption.nextNodeId`, `RandomOption.nextNodeId`
- BaseGraph
  - `ScriptableObject`
  - serializable runtime container for base pipeline

## Verification
- create asset: pending manual verification in Unity
- add nodes: pending manual verification in Unity
- save asset: pending manual verification in Unity
- reopen asset: pending manual verification in Unity

## Remaining Issues
- none confirmed from code path
- if NRE persists, capture fresh stack trace after this importer change

## Notes
- Likely root cause: importer set `RuntimeGraph` as main object for `.basegraph`, while GraphToolkit save watcher expects a graph object with valid `GraphModel`.
- Fix: when `BaseGraphEditorGraph` is available, set it as main object and keep runtime graph as sub-asset.
