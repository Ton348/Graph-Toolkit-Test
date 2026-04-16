using GraphCore.Runtime;

public static class GraphContextKeys
{
    public static readonly GraphContextKey<GameBootstrap> runtimeBootstrap = new GraphContextKey<GameBootstrap>("runtime.bootstrap");
    public static readonly GraphContextKey<MapMarkerService> runtimeMapMarkerService = new GraphContextKey<MapMarkerService>("runtime.mapMarkerService");
    public static readonly GraphContextKey<UnityEngine.Transform> runtimePlayerTransform = new GraphContextKey<UnityEngine.Transform>("runtime.playerTransform");
    public static readonly GraphContextKey<int> choiceLastIndex = new GraphContextKey<int>("choice.lastIndex");
    public static readonly GraphContextKey<string> choiceLastLabel = new GraphContextKey<string>("choice.lastLabel");
    public static readonly GraphContextKey<bool> conditionLastResult = new GraphContextKey<bool>("condition.lastResult");
    public static readonly GraphContextKey<ServerActionResult> serverLastResult = new GraphContextKey<ServerActionResult>("server.lastResult");
    public static readonly GraphContextKey<string> buildingLastRequestedId = new GraphContextKey<string>("building.lastRequestedId");
    public static readonly GraphContextKey<string> questLastRequestedId = new GraphContextKey<string>("quest.lastRequestedId");
}
