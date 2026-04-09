# Wave 3 Implementation Report

## Implemented Files
- Assets/GraphCore/Runtime/BaseNodes/World/MapMarkerNode.cs
- Assets/GraphCore/Runtime/BaseNodes/Cinematics/PlayCutsceneNode.cs
- Assets/GraphCore/Editor/BaseNodes/World/MapMarkerNodeModel.cs
- Assets/GraphCore/Editor/BaseNodes/Cinematics/PlayCutsceneNodeModel.cs

## Runtime Notes
- MapMarkerNode
  - Clean data only: `markerId`, `targetObjectName`, `nextNodeId` (inherited from `BusinessQuestNode`).
  - No business/prototype dependencies.
- PlayCutsceneNode
  - Clean data only: `cutsceneReference`, `nextNodeId` (inherited from `BusinessQuestNode`).
  - No business/prototype dependencies.

## Editor Notes
- MapMarkerNodeModel
  - Options: `MarkerId`, `Target`.
  - Ports: execution input + execution output.
  - Title/Description per spec.
  - Registered on `BaseGraphEditorGraph`.
- PlayCutsceneNodeModel
  - Option: `CutsceneReference`.
  - Ports: execution input + execution output.
  - Title/Description per spec.
  - Registered on `BaseGraphEditorGraph`.

## Reference Usage
- Donor/reference used: legacy `AddMapMarkerNodeModel`.
- Reused from donor:
  - Marker option idea (`MarkerId`).
  - Execution in/out port layout.
- Intentionally NOT reused:
  - Legacy class name (`AddMapMarker...`).
  - Legacy base class coupling (`BusinessQuestCommonNodeModel`).
  - Runtime `Transform` dependency and legacy/service-specific behavior.

## Importer Changes
- Updated `Assets/GraphCore/Editor/BaseGraphImporter.cs`:
  - Added model/runtime mapping:
    - `MapMarkerNodeModel -> MapMarkerNode`
    - `PlayCutsceneNodeModel -> PlayCutsceneNode`
  - Added option mapping:
    - `MarkerId -> markerId`
    - `Target -> targetObjectName`
    - `CutsceneReference -> cutsceneReference`
  - `nextNodeId` mapping handled by existing default connection path.

## Runner Changes
- Updated `Assets/GraphCore/Runtime/BaseGraphRunner.cs`:
  - Added execution case for `MapMarkerNode`:
    - uses `GraphExecutionContext.MapMarkerService` if available
    - otherwise safe `Debug.Log` fallback
    - continues to `nextNodeId`
  - Added execution case for `PlayCutsceneNode`:
    - uses `GraphExecutionContext.CutsceneService` if available (`await PlayAsync`)
    - otherwise safe `Debug.Log` fallback
    - continues to `nextNodeId`
- Updated `Assets/GraphCore/Runtime/GraphExecutionContext.cs`:
  - Added clean service interfaces:
    - `IGraphMapMarkerService`
    - `IGraphCutsceneService`
  - Added context properties:
    - `MapMarkerService`
    - `CutsceneService`

## Compile Result
- has errors: not detected from CLI in this step
- Unity compile status must be verified in Editor (errors list not available from terminal-only check)

## Follow-up Notes
- For real service integration:
  - Provide concrete adapters implementing `IGraphMapMarkerService` and `IGraphCutsceneService`.
  - Inject them into `GraphExecutionContext` where `BaseGraphRunner.RunAsync(...)` is launched.
