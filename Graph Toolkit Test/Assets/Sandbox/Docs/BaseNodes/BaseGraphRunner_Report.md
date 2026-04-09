# BaseGraphRunner Report

## Implemented Methods
- `Start(BusinessQuestGraph graph)`
- `Stop()`
- `Tick()`
- internal `Advance()`

## Supported Nodes
- `GraphCore.BaseNodes.Runtime.Flow.StartNode`
- `GraphCore.BaseNodes.Runtime.Flow.FinishNode`
- `GraphCore.BaseNodes.Runtime.UI.DialogueNode`
- `GraphCore.BaseNodes.Runtime.UI.ChoiceNode`
- `GraphCore.BaseNodes.Runtime.Utility.LogNode`
- `GraphCore.BaseNodes.Runtime.Flow.DelayNode`
- `GraphCore.BaseNodes.Runtime.Flow.RandomNode`

## Missing Dependencies by Design
- No `GameBootstrap`
- No `IGameServer`
- No `ProfileSyncService`
- No `DialogueService` UI integration
- No `ChoiceUIService` UI integration
- No sandbox/business services

## Can Run Without Sandbox
- Yes, for a minimal base-node-only graph path.
- Dialogue and Choice are temporary non-UI implementations (`Debug.Log` + first valid option selection).
