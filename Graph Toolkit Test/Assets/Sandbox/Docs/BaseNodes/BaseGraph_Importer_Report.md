# BaseGraph Importer Report

## importer path
- Added clean importer output path in `Assets/GraphCore/Editor/BaseGraphImporter.cs`.
- New entry method: `Graph.Core.Editor.BaseGraphImporter.BuildBaseGraph(BusinessQuestEditorGraph graph)`.
- Output type: `BaseGraph`.

## supported editor models
- `StartNodeModel`
- `FinishNodeModel`
- `DialogueNodeModel`
- `ChoiceNodeModel`
- `LogNodeModel`
- `DelayNodeModel`
- `RandomNodeModel`

## runtime conversion
- `StartNodeModel` -> `StartNode`
- `FinishNodeModel` -> `FinishNode`
- `DialogueNodeModel` -> `DialogueNode`
- `ChoiceNodeModel` -> `ChoiceNode`
- `LogNodeModel` -> `LogNode`
- `DelayNodeModel` -> `DelayNode`
- `RandomNodeModel` -> `RandomNode`

## choice/random mapping
- Choice:
  - `Option1..Option4` labels -> `ChoiceNode.options[i].label`
  - output port connections -> `ChoiceNode.options[i].nextNodeId`
- Random:
  - `Weight1..Weight4` -> `RandomNode.options[i].weight`
  - output port connections -> `RandomNode.options[i].nextNodeId`

## legacy safety
- Legacy business importer path was not modified in behavior.
- Sandbox/prototype runtime layer was not modified by this step.
