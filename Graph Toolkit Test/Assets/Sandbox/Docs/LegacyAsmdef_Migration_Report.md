# Legacy Asmdef Migration Report

## 1. Created asmdef
- path: `Assets/Scripts/Legacy.Runtime.asmdef`
  - assembly name: `Legacy.Runtime`
  - type: runtime
  - references:
    - `GraphCore.Runtime`
    - `Game1Graph.Runtime`
- path: `Assets/Editor/Legacy.Editor.asmdef`
  - assembly name: `Legacy.Editor`
  - type: editor-only
  - references:
    - `Legacy.Runtime`

## 2. Updated asmdef
- path: `Assets/Sandbox/Sandbox.asmdef`
  - added references:
    - `Legacy.Runtime`
  - removed references:
    - none

## 3. Assembly Result
- `Assembly-CSharp`: **остался**
  - сейчас туда попадают в основном файлы вне asmdef-корней, например:
    - `Assets/Art/DavidJalbert/LowPolyPeople/Example/Scripts/AnimationController.cs`
    - `Assets/TutorialInfo/Scripts/Readme.cs`
- `Assembly-CSharp-Editor`: **остался**
  - сейчас туда попадают, например:
    - `Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs`
- Папки, попавшие в `Legacy.Runtime`:
  - `Assets/Scripts/**` (runtime код в этой ветке)
- Папки, попавшие в `Legacy.Editor`:
  - `Assets/Editor/**`

## 4. Cyclic Dependency Status
- cyclic dependency: **есть**
- assemblies из лога:
  - `Assembly-CSharp-Editor`, `Assembly-CSharp`, `Game1Graph.Editor`, `Game1Graph.Runtime`, `Sandbox`
  - в одном из сообщений также фигурировал `GraphCore.Editor`
- предполагаемая цепочка цикла:
  - `Assembly-CSharp-Editor -> Assembly-CSharp -> Sandbox -> Game1Graph.Runtime -> Game1Graph.Editor -> (обратно через predefined assemblies)`

## 5. Current Compile Errors
- total errors (уникальные строки в срезе `Editor.log`): **246**

Ошибки по assembly (по префиксам путей):
- `GraphCore.Runtime`: 52
- `GraphCore.Editor`: 31
- `Game1Graph.Runtime`: 22
- `Game1Graph.Editor`: 0
- `Sandbox`: 141
- `Legacy.Runtime`: 0
- `Legacy.Editor`: 0

Top 10 files by error count:
1. `Assets/Sandbox/Services/LocalGameServer.cs` — 44
2. `Assets/GraphCore/Runtime/BusinessQuestGraphRunner.cs` — 31
3. `Assets/Sandbox/Services/RemoteGameServer.cs` — 28
4. `Assets/Sandbox/Services/IGameServer.cs` — 25
5. `Assets/Game1Graph/Runtime/BusinessQuestGraphRunner.cs` — 22
6. `Assets/GraphCore/Editor/BusinessQuestGraphImporter.cs` — 15
7. `Assets/GraphCore/Runtime/GraphCoreRuntimeApi.cs` — 14
8. `Assets/Sandbox/BusinessQuestGraphRunner.cs` — 11
9. `Assets/Sandbox/Services/PlayerStateSync.cs` — 8
10. `Assets/GraphCore/Editor/BusinessQuestEditorNode.cs` — 7

## 6. Recommendations
- Основной узел проблем сейчас в `Sandbox`: там сконцентрированы типы, зависящие от legacy runtime DTO/сервисов и UI.
- В логе остаются старые пути (`Assets/GraphCore/Runtime/*`, `Assets/GraphCore/Editor/*`) как legacy шум после предыдущих переносов; нужен отдельный clean-snapshot ошибок после полного стабильного recompile.
- `GraphCore.Runtime` по структуре остаётся чистым (в папке только core-файлы), но ошибки по старым путям ещё загрязняют картину.
- Для `GraphCore.Editor` остаётся unresolved зависимость по `Unity.GraphToolkit.*`/`Node`-типам; нужно отдельно валидировать корректные package assembly names и доступность API версии.
- Цикл по-прежнему включает predefined assemblies; до его снятия любые точечные правки reference дают ограниченный эффект.
- Следующий безопасный шаг: диагностически изолировать зависимости `Sandbox <-> Game1Graph.Runtime` и пути попадания в `Assembly-CSharp`/`Assembly-CSharp-Editor` для оставшихся не-asmdef участков.
