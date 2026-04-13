# Game1.Graph / GameGraph Base Architecture Report

## Created in Game1.Graph
- `Assets/Modules/Game1.Graph/Runtime/Infrastructure/GameGraphNodeExecutor.cs`
- `Assets/Modules/Game1.Graph/Runtime/Infrastructure/IGameGraphNodeConverter.cs`
- `Assets/Modules/Game1.Graph/Runtime/Infrastructure/GameGraphNodeConverterRegistry.cs`
- `Assets/Modules/Game1.Graph/Runtime/Infrastructure/GameGraphExecutorRegistry.cs`
- `Assets/Modules/Game1.Graph/Runtime/Infrastructure/GameGraphComposition.cs`

## Existing/verified in Game1.Graph
- `Assets/Modules/Game1.Graph/Runtime/GameGraphNode.cs`
- `Assets/Modules/Game1.Graph/Editor/GameGraphEditorNode.cs`

## Created in GameGraph
- `Assets/GameGraph/Runtime/Infrastructure/GameRuntimeNodeConverterRegistration.cs`
- `Assets/GameGraph/Runtime/Infrastructure/GameRuntimeExecutorRegistration.cs`
- `Assets/GameGraph/Runtime/Infrastructure/GameRuntimeComposition.cs`
- `Assets/GameGraph/Editor/Infrastructure/GameEditorNodeModelRegistration.cs`

## Registry / Composition Responsibilities
- `GameGraphNodeConverterRegistry`: runtime registry for editor-node-model to runtime-node converters.
- `GameGraphExecutorRegistry`: runtime registry for game-specific executors.
- `GameGraphComposition`: reusable game-layer composition point combining core executors with registered game executors and exposing converter registry.
- `GameRuntimeNodeConverterRegistration`: game-level place to register concrete converters for this game.
- `GameRuntimeExecutorRegistration`: game-level place to register concrete executors for this game.
- `GameRuntimeComposition`: game-level assembly entry point that builds `GameGraphComposition` with game registrations.
- `GameEditorNodeModelRegistration`: editor-side hook point for future game-specific editor registration helpers.

## asmdef Status
- `Game1.Graph.Runtime` -> `Graph.Core.Runtime` (ok)
- `Game1.Graph.Editor` -> `Game1.Graph.Runtime`, `Graph.Core.Runtime`, `Graph.Core.Editor`, GraphToolkit editor assemblies (ok)
- `GameGraph.Runtime` -> `Game1.Graph.Runtime`, `Graph.Core.Runtime` (ok)
- `GameGraph.Editor` -> `GameGraph.Runtime`, `Game1.Graph.Runtime`, `Game1.Graph.Editor`, `Graph.Core.Runtime`, `Graph.Core.Editor`, GraphToolkit editor assemblies (ok)

## Readiness
- Base extension architecture for new game nodes without `Graph.Core` changes: **ready**
- Required per new node remains:
	- runtime node
	- editor node model
	- executor
	- converter
	- registration in `GameRuntimeExecutorRegistration` / `GameRuntimeNodeConverterRegistration`
