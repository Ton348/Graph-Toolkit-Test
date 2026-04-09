# Assembly Cycle Diagnostic

## 1. Physical File Locations
- `BusinessQuestGraphRunner.cs`
  - `Assets/Sandbox/BusinessQuestGraphRunner.cs`
- `BusinessQuestGraph.cs`
  - `Assets/Game1Graph/Runtime/BusinessQuestGraph.cs`
- `GraphCoreRuntimeApi.cs`
  - `Assets/Sandbox/GraphCoreRuntimeApi.cs`
- `InteractionContext.cs`
  - `Assets/Sandbox/InteractionContext.cs`
- `GraphContextKeys.cs`
  - `Assets/Sandbox/GraphContextKeys.cs`
- `GraphExecutionEvent.cs`
  - `Assets/Sandbox/GraphExecutionEvent.cs`
- `BusinessQuestGraphImporter.cs`
  - `Assets/Game1Graph/Editor/BusinessQuestGraphImporter.cs`
- `ConditionNodeViewDecorator.cs`
  - `Assets/Game1Graph/Editor/ConditionNodeViewDecorator.cs`

## 2. Duplicate Files
- По целевым 8 файлам физических дублей в `Assets/` не найдено (по имени файла — по 1 актуальному пути на каждый).

## 3. Folder -> Assembly Map

### `Assets/GraphCore`
- asmdef: yes
- assemblies:
  - `GraphCore.Runtime` (`Assets/GraphCore/Runtime/GraphCore.Runtime.asmdef`)
  - `GraphCore.Editor` (`Assets/GraphCore/Editor/GraphCore.Editor.asmdef`)
- key files:
  - Runtime: `BusinessQuestNode.cs`, `GraphExecutionContext.cs`, `GraphContextKey.cs`
  - Editor: `BusinessQuestEditorGraph.cs`, `BusinessQuestEditorNode.cs`, `GraphCoreEditorApi.cs`

### `Assets/Game1Graph`
- asmdef: yes
- assemblies:
  - `Game1Graph.Runtime` (`Assets/Game1Graph/Runtime/Game1Graph.Runtime.asmdef`)
  - `Game1Graph.Editor` (`Assets/Game1Graph/Editor/Game1Graph.Editor.asmdef`)
- key files:
  - Runtime: `BusinessQuestGraph.cs`
  - Editor: `BusinessQuestGraphImporter.cs`, `ConditionNodeViewDecorator.cs`

### `Assets/Sandbox`
- asmdef: yes
- assembly:
  - `Sandbox` (`Assets/Sandbox/Sandbox.asmdef`)
- key files:
  - `BusinessQuestGraphRunner.cs`, `GraphCoreRuntimeApi.cs`, `GraphContextKeys.cs`, `GraphExecutionEvent.cs`, `InteractionContext.cs`
  - `Services/*`, `UI/*`, `Compass/*`

### `Assets/Scripts`
- asmdef: no
- assembly: `Assembly-CSharp` (runtime) и `Assembly-CSharp-Editor` (файлы в `Editor/`)
- key files:
  - Runtime: `Bootstrap/GameBootstrap.cs`, `Services/*`, `Runtime/*`, `GameData/*`, `Tools/Graph/Core/Runtime/{ConditionEvaluator.cs,StealContextEvaluator.cs}`, `UI/TradeOfferUIService.cs`, `Compass/*`
  - Editor: `Assets/Editor/*`

### `Assets/Scripts/Tools/Graph`
- asmdef: no
- assembly: `Assembly-CSharp`
- key files:
  - `Assets/Scripts/Tools/Graph/Core/Runtime/ConditionEvaluator.cs`
  - `Assets/Scripts/Tools/Graph/Core/Runtime/StealContextEvaluator.cs`

### `Assets/Scripts/Services`
- asmdef: no
- assembly: `Assembly-CSharp`
- key files:
  - `RequestManager.cs`, `BuildingService.cs`, `ProfileSyncService.cs`, `MapMarkerService.cs`, `ServerActionResult.cs`, `PlayerService.cs`

### `Assets/Scripts/UI`
- asmdef: no
- assembly: `Assembly-CSharp`
- key files:
  - `BuildingStatusWindow.cs`, `MoneyUI.cs`, `TradeOfferUIService.cs`

### `Assets/Scripts/Compass`
- asmdef: no
- assembly: `Assembly-CSharp`
- key files:
  - `CompassManager.cs`, `CompassUIController.cs`, `CompassTarget*.cs`, `CompassMarkerView.cs`, `CompassTickView.cs`

## 4. Files In Assembly-CSharp
Основные группы (без asmdef, не в `Editor/`):
- `Assets/Scripts/**` runtime
- `Assets/Scripts/Tools/Graph/Core/Runtime/{ConditionEvaluator.cs,StealContextEvaluator.cs}`
- `Assets/Art/.../Example/Scripts/AnimationController.cs`
- `Assets/TutorialInfo/Scripts/Readme.cs`

## 5. Files In Assembly-CSharp-Editor
Основные группы (без asmdef, в `Editor/`):
- `Assets/Editor/*.cs`
- `Assets/TutorialInfo/Scripts/Editor/*.cs`

## 6. Current Cyclic Dependency Analysis
Лог содержит цикл:
- `Assembly-CSharp-Editor, Assembly-CSharp, Game1Graph.Editor, Game1Graph.Runtime, Sandbox` (ранее также фигурировал `GraphCore.Editor`).

Вероятная цепочка:
1. `Assembly-CSharp-Editor` зависит от `Assembly-CSharp`.
2. `Assembly-CSharp` как predefined assembly участвует вместе с autoReferenced asmdef (`Sandbox`, `Game1Graph.Runtime`).
3. `Sandbox` ссылается на `Game1Graph.Runtime`.
4. `Game1Graph.Editor` ссылается на `Game1Graph.Runtime`.
5. Одновременно есть зависимости к типам из `Assembly-CSharp` (legacy `Assets/Scripts/*`) из asmdef-сборок, что замыкает цикл с predefined assemblies.

## 7. Current Compile Errors
Источник: последние 1000 строк `Editor.log`, уникальные ошибки: **246**.

### duplicate file/path issues
- Явных duplicate file/path ошибок по C# не найдено.

### cyclic dependency issues
- Есть системная ошибка о cyclic dependencies между predefined и asmdef сборками.

### missing assembly reference issues
- Присутствуют `CS0234`, `CS0400` (пример: `Unity.GraphToolkit` namespace в `GraphCore.Editor`; `global::...` типы в bridge/API файлах).

### missing type issues
- Присутствуют `CS0246` массово (service/UI/game DTO/runtime types not found).

### legacy old-path issues
- В логе остаются ошибки по старым путям после переносов:
  - `Assets/GraphCore/Runtime/BusinessQuestGraphRunner.cs`
  - `Assets/GraphCore/Runtime/BusinessQuestGraph.cs`
  - `Assets/GraphCore/Runtime/GraphCoreRuntimeApi.cs`
  - `Assets/GraphCore/Runtime/InteractionContext.cs`
  - `Assets/GraphCore/Runtime/GraphContextKeys.cs`
  - `Assets/GraphCore/Runtime/GraphExecutionEvent.cs`
  - `Assets/GraphCore/Editor/BusinessQuestGraphImporter.cs`
  - `Assets/GraphCore/Editor/ConditionNodeViewDecorator.cs`

## 8. Top Recommendations
- Стабилизировать границу между asmdef и `Assembly-CSharp` (минимизировать прямые зависимости asmdef-кода на `Assets/Scripts/*`).
- Довести перенос bridge/runtime файлов до единых актуальных путей и дождаться полного clean reimport, чтобы убрать legacy old-path шум в логе.
- Разорвать цикл на уровне зависимостей `Sandbox <-> Game1Graph.Runtime` с учетом фактических типов, которые сейчас остаются в legacy `Assembly-CSharp`.
- Уточнить корректные GraphToolkit assembly references для editor-слоя (текущие `Unity.GraphToolkit.*` unresolved по логу).
- После чистки зависимостей заново снять "чистый" error snapshot (без исторических записей) и использовать его как baseline.
