public static class GraphRuntimeContextKeys
{
	public static readonly GraphContextKey<bool> immediateChoiceAfterDialogue = new GraphContextKey<bool>("graph.runtime.immediateChoiceAfterDialogue");
	public static readonly GraphContextKey<CommonGraph> currentGraph = new GraphContextKey<CommonGraph>("graph.runtime.currentGraph");
}
