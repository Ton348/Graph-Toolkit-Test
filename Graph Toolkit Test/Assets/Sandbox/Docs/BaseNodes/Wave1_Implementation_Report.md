# Wave 1 Implementation Report

## Implemented Files
- `Assets/GraphCore/Runtime/BaseNodes/Flow/StartNode.cs`
- `Assets/GraphCore/Runtime/BaseNodes/Flow/FinishNode.cs`
- `Assets/GraphCore/Runtime/BaseNodes/UI/DialogueNode.cs`
- `Assets/GraphCore/Runtime/BaseNodes/UI/ChoiceNode.cs`
- `Assets/GraphCore/Runtime/BaseNodes/UI/ChoiceOption.cs`
- `Assets/GraphCore/Editor/BaseNodes/Flow/StartNodeModel.cs`
- `Assets/GraphCore/Editor/BaseNodes/Flow/FinishNodeModel.cs`
- `Assets/GraphCore/Editor/BaseNodes/UI/DialogueNodeModel.cs`
- `Assets/GraphCore/Editor/BaseNodes/UI/ChoiceNodeModel.cs`

## Runtime Notes
- StartNode
  - Clean runtime node with no custom fields.
  - Default title/description set to `Start` / `Entry point of the graph`.
- FinishNode
  - Clean termination node with no custom fields.
  - No checkpoint/quest/server logic included.
- DialogueNode
  - Clean fields: `dialogueTitle`, `body`.
  - No screenshot/media fields added in Wave 1.
- ChoiceNode
  - Uses clean `ChoiceOption` list with 4 predefined slots.
  - No business/prototype behavior embedded.
- ChoiceOption
  - Contains only `label` and `nextNodeId`.

## Editor Notes
- StartNodeModel
  - Output execution port only.
  - Title/description follow Wave 1 spec.
- FinishNodeModel
  - Input execution port only.
  - Title/description follow Wave 1 spec.
- DialogueNodeModel
  - Options: `Title`, `Body`.
  - Input + output execution ports.
- ChoiceNodeModel
  - Options: `Option1..Option4`.
  - Input + 4 output execution ports.

## Dependencies Used
- Runtime base class: `BusinessQuestNode` (`Assets/GraphCore/Runtime/BusinessQuestNode.cs`)
- Editor base class: `Graph.Core.Editor.GraphEditorNode` (`Assets/GraphCore/Editor/GraphCoreEditorApi.cs`)
- Graph binding attribute: `UseWithGraph(typeof(Graph.Core.Editor.GraphEditorGraph))`
- Graph Toolkit editor contracts: `IPortDefinitionContext`, `IOptionDefinitionContext`, `PortConnectorUI`

## Compile Result
- has errors (project-level legacy errors remain)
- new Wave 1 files: no compile errors found in `Editor.log` for the new file paths

## Follow-up Notes
- Runtime execution behavior for new Wave 1 nodes is not wired into current legacy runner on this step by design.
- For full runtime execution, next step needs a clean base runner/executor path for `GraphCore.BaseNodes.*` types.
- Current project compile noise is dominated by legacy/module split issues outside Wave 1 files.
