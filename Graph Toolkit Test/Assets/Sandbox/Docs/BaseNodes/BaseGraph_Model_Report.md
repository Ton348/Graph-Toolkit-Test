# BaseGraph Model Report

## Created
- `Assets/GraphCore/Runtime/BaseGraph.cs`

## BaseGraph
- Minimal container for base-node execution.
- Fields:
  - `startNodeId`
  - `List<BusinessQuestNode> nodes`
- Methods:
  - `GetNodeById(string id)`
  - `GetStartNode()`

## Updated
- `Assets/GraphCore/Runtime/BaseGraphRunner.cs`
  - Replaced `BusinessQuestGraph` with `BaseGraph` in runner state and `Start(...)` signature.

## Notes
- No changes to `BusinessQuestGraph`, legacy runner, importer, Sandbox, or business/prototype code.
- BaseGraph intentionally contains only core-safe traversal methods and no business/checkpoint/quest behavior.
