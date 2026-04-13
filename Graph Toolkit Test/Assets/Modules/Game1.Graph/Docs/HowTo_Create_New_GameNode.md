# Как создать новую игровую ноду (Game Node)

## 1. Создать runtime-ноду
Путь: `Runtime/`

- Наследоваться от базового класса:
  - `GameGraphNode` — если нужна кастомная логика
  - или от готовых шаблонов:
    - `GameGraphNextNode` — линейная нода с `nextNodeId`
    - `GameGraphSuccessFailNode` — ветвление success/fail
    - `GameGraphTrueFalseNode` — ветвление true/false

---

## 2. Создать editor-ноду (NodeModel)
Путь: `Editor/`

- Наследоваться от:
  - `GameGraphEditorNode`
  - или специализированных моделей
- Обязательно добавить атрибут:
```csharp
[UseWithGraph(typeof(CommonGraphEditorGraph))]
```
- Переопределить:
  - `DefaultTitle`
  - `DefaultDescription`
- Описать:
  - Options (данные ноды)
  - Ports (входы/выходы)

---

## 3. Создать executor (runtime выполнение)
Путь: `Runtime/`

- Наследоваться от:
```csharp
GameGraphNodeExecutor<TNode>
```

- Или использовать шаблоны:
  - `GameGraphNextNodeExecutor<TNode>`
  - `GameGraphSuccessFailNodeExecutor<TNode>`
  - `GameGraphTrueFalseNodeExecutor<TNode>`

Executor отвечает за выполнение ноды в runtime.

---

## 4. Создать converter
Путь: `Editor/`

- Наследоваться от:
```csharp
GameGraphNodeConverterBase<TModel, TNode>
```

- Задача converter:
  - преобразовать Editor NodeModel → Runtime Node
  - считать значения из Options

---

## 5. Регистрация executor

Если используется авто-регистрация:
- добавить атрибут:
```csharp
[GameGraphNodeExecutor]
```

Если вручную:
- через `GameGraphExecutorRegistry`

---

## 6. Регистрация converter

Если используется авто-регистрация:
- добавить атрибут:
```csharp
[GameGraphNodeConverter]
```

Если вручную:
- через `GameGraphNodeConverterRegistry`

---

## 7. Валидация (опционально, но рекомендуется)

- Создать валидатор:
```csharp
IGameGraphNodeValidator
```
- Добавить атрибут:
```csharp
[GameGraphNodeValidator]
```

Валидация выполняется при сборке графа и может блокировать build.

---

## 8. Проверка

После создания ноды:

1. Нода появилась в editor
2. Граф сохраняется
3. Runtime asset собирается
4. Нода выполняется в Play Mode

---

## Итоговый pipeline

```text
Editor NodeModel
    ↓
Converter
    ↓
Runtime Node
    ↓
Executor
    ↓
Выполнение в игре
```

---

## Важно

- `Graph.Core` — не трогать без необходимости
- `Game1.Graph` — только framework
- игровые ноды должны лежать в game-слое
