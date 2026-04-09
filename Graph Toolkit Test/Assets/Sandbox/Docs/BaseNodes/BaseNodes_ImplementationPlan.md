# Base Nodes Implementation Plan

## Wave 1
- StartNode
- FinishNode
- DialogueNode
- ChoiceNode

## Wave 2
- LogNode
- DelayNode
- RandomNode

## Wave 3
- MapMarkerNode
- PlayCutsceneNode

## Wave 4
- CheckpointNode
- StartQuestNode
- CompleteQuestNode
- QuestStateConditionNode

## Rules
- New base nodes are implemented in GraphCore/BaseNodes
- Legacy/prototype nodes are not modified on this step
- Business nodes are sandbox-only
- FinishNode must be clean and not reuse legacy EndNode quest/checkpoint logic
- ChoiceNode should be rebuilt around target base behavior, even if partial reuse is possible

## Wave 1 Planned Files

### Runtime
- StartNode.cs — Reuse as reference
- FinishNode.cs — New
- DialogueNode.cs — Reuse/Rework
- ChoiceNode.cs — Rework
- ChoiceOption.cs — Rework/New

### Editor
- StartNodeModel.cs — Reuse as reference
- FinishNodeModel.cs — New
- DialogueNodeModel.cs — Reuse/Rework
- ChoiceNodeModel.cs — Rework
