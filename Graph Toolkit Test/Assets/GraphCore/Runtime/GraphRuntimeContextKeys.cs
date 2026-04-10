public static class GraphRuntimeContextKeys
{
    public static readonly GraphContextKey<bool> immediateChoiceAfterDialogue = new GraphContextKey<bool>("graph.runtime.immediateChoiceAfterDialogue");
    public static readonly GraphContextKey<BaseGraph> currentGraph = new GraphContextKey<BaseGraph>("graph.runtime.currentGraph");
}
