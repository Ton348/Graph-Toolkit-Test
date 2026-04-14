public static class GraphContextKeys
{
    public static readonly GraphContextKey<GameBootstrap> RuntimeBootstrap = new GraphContextKey<GameBootstrap>("runtime.bootstrap");
    public static readonly GraphContextKey<MapMarkerService> RuntimeMapMarkerService = new GraphContextKey<MapMarkerService>("runtime.mapMarkerService");
    public static readonly GraphContextKey<UnityEngine.Transform> RuntimePlayerTransform = new GraphContextKey<UnityEngine.Transform>("runtime.playerTransform");
    public static readonly GraphContextKey<int> ChoiceLastIndex = new GraphContextKey<int>("choice.lastIndex");
    public static readonly GraphContextKey<string> ChoiceLastLabel = new GraphContextKey<string>("choice.lastLabel");
    public static readonly GraphContextKey<bool> ConditionLastResult = new GraphContextKey<bool>("condition.lastResult");
    public static readonly GraphContextKey<ServerActionResult> ServerLastResult = new GraphContextKey<ServerActionResult>("server.lastResult");
    public static readonly GraphContextKey<string> BuildingLastRequestedId = new GraphContextKey<string>("building.lastRequestedId");
    public static readonly GraphContextKey<string> QuestLastRequestedId = new GraphContextKey<string>("quest.lastRequestedId");
}
