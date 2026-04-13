# How To Create New Game Node

1. Create runtime node in `Runtime/`.
- Inherit from `GameGraphNode` or one of templates:
- `GameGraphNextNode`
- `GameGraphSuccessFailNode`
- `GameGraphTrueFalseNode`

2. Create editor model in `Editor/`.
- Inherit from `GameGraphEditorNode` or template models.
- Add `[UseWithGraph(typeof(CommonGraphEditorGraph))]`.
- Define `DefaultTitle` and `DefaultDescription`.
- Define options and ports.

3. Create executor in `Runtime/`.
- Inherit from `GameGraphNodeExecutor<TNode>`.
- Or use template executors:
- `GameGraphNextNodeExecutor<TNode>`
- `GameGraphSuccessFailNodeExecutor<TNode>`
- `GameGraphTrueFalseNodeExecutor<TNode>`

4. Create converter in `Editor/`.
- Inherit from `GameGraphNodeConverterBase<TModel, TNode>`.
- Read options via helper methods.

5. Register executor.
- Use `GameGraphExecutorRegistry.Register(...)` or `Register<TExecutor>()`.

6. Register converter.
- Use `GameGraphNodeConverterRegistry.Register(...)` or `Register<TConverter>()`.

7. Build composition.
- Runtime: `GameGraphComposition`.
- Editor/import: `GameGraphEditorComposition`.
