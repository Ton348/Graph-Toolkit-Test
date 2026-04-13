# GameGraph Step 2 Migration Report

## moved runtime files
- `Assets/Modules/Game1.Graph/Runtime/Common/ConditionNode.cs` -> `Assets/GameGraph/Runtime/Common/ConditionNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Common/ConditionType.cs` -> `Assets/GameGraph/Runtime/Common/ConditionType.cs`
- `Assets/Modules/Game1.Graph/Runtime/Common/GoToPointNode.cs` -> `Assets/GameGraph/Runtime/Common/GoToPointNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Common/PlayerStatType.cs` -> `Assets/GameGraph/Runtime/Common/PlayerStatType.cs`
- `Assets/Modules/Game1.Graph/Runtime/Common/SetGameObjectActiveNode.cs` -> `Assets/GameGraph/Runtime/Common/SetGameObjectActiveNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Common/SkillType.cs` -> `Assets/GameGraph/Runtime/Common/SkillType.cs`
- `Assets/Modules/Game1.Graph/Runtime/Quest/QuestActionType.cs` -> `Assets/GameGraph/Runtime/Quest/QuestActionType.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/CheckBusinessExistsNode.cs` -> `Assets/GameGraph/Runtime/Business/CheckBusinessExistsNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/CheckBusinessModuleInstalledNode.cs` -> `Assets/GameGraph/Runtime/Business/CheckBusinessModuleInstalledNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/CheckBusinessOpenNode.cs` -> `Assets/GameGraph/Runtime/Business/CheckBusinessOpenNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/CheckContactKnownNode.cs` -> `Assets/GameGraph/Runtime/Business/CheckContactKnownNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestAssignBusinessTypeNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestAssignBusinessTypeNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestAssignSupplierNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestAssignSupplierNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestBuyBuildingNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestBuyBuildingNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestCloseBusinessNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestCloseBusinessNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestHireBusinessWorkerNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestHireBusinessWorkerNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestInstallBusinessModuleNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestInstallBusinessModuleNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestOpenBusinessNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestOpenBusinessNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestRentBusinessNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestRentBusinessNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestSetBusinessMarkupNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestSetBusinessMarkupNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestSetBusinessOpenNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestSetBusinessOpenNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestTradeOfferNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestTradeOfferNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/Business/RequestUnlockContactNode.cs` -> `Assets/GameGraph/Runtime/Business/RequestUnlockContactNode.cs`
- `Assets/Modules/Game1.Graph/Runtime/BusinessQuestGraph.cs` -> `Assets/GameGraph/Runtime/BusinessQuestGraph.cs`

## created editor files
- `Assets/GameGraph/Editor/Common/ConditionNodeModel.cs`
- `Assets/GameGraph/Editor/Common/GoToPointNodeModel.cs`
- `Assets/GameGraph/Editor/Common/SetGameObjectActiveNodeModel.cs`
- `Assets/GameGraph/Editor/Business/GameGraphSuccessFailNodeModel.cs`
- `Assets/GameGraph/Editor/Business/GameGraphTrueFalseNodeModel.cs`
- `Assets/GameGraph/Editor/Business/CheckBusinessExistsNodeModel.cs`
- `Assets/GameGraph/Editor/Business/CheckBusinessModuleInstalledNodeModel.cs`
- `Assets/GameGraph/Editor/Business/CheckBusinessOpenNodeModel.cs`
- `Assets/GameGraph/Editor/Business/CheckContactKnownNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestAssignBusinessTypeNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestAssignSupplierNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestBuyBuildingNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestCloseBusinessNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestHireBusinessWorkerNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestInstallBusinessModuleNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestOpenBusinessNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestRentBusinessNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestSetBusinessMarkupNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestSetBusinessOpenNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestTradeOfferNodeModel.cs`
- `Assets/GameGraph/Editor/Business/RequestUnlockContactNodeModel.cs`

## asmdef status
- `Game1.Graph.Runtime`: unchanged (`Graph.Core.Runtime`)
- `Game1.Graph.Editor`: present and configured
- `GameGraph.Runtime`: configured (`Game1.Graph.Runtime`, `Graph.Core.Runtime`)
- `GameGraph.Editor`: configured (`GameGraph.Runtime`, `Game1.Graph.*`, `Graph.Core.*`, GraphToolkit editor assemblies)

## visibility in graph editor
- All new GameGraph node models are marked with:
	- `[UseWithGraph(typeof(CommonGraphEditorGraph))]`
- They are expected to appear in the same graph editor as Graph.Core base nodes.

## compile result
- Not executed in CLI (Unity compile step not run from this environment).

## notes about BusinessQuestGraph
- `BusinessQuestGraph.cs` was treated as game-specific graph container and moved to `Assets/GameGraph/Runtime`.
- `Assets/Modules/Game1.Graph` now keeps only reusable extension layer (`GameGraphNode`, `GameGraphEditorNode`, asmdef).
