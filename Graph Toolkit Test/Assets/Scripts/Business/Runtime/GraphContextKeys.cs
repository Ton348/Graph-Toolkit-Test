using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Services;
using Sample.Runtime.Services;
using UnityEngine;

namespace Prototype.Business.Runtime
{
	public static class GraphContextKeys
	{
		public static readonly GraphContextKey<GameBootstrap> runtimeBootstrap = new("runtime.bootstrap");
		public static readonly GraphContextKey<MapMarkerService> runtimeMapMarkerService = new("runtime.mapMarkerService");
		public static readonly GraphContextKey<Transform> runtimePlayerTransform = new("runtime.playerTransform");
		public static readonly GraphContextKey<int> choiceLastIndex = new("choice.lastIndex");
		public static readonly GraphContextKey<string> choiceLastLabel = new("choice.lastLabel");
		public static readonly GraphContextKey<bool> conditionLastResult = new("condition.lastResult");
		public static readonly GraphContextKey<ServerActionResult> serverLastResult = new("server.lastResult");
		public static readonly GraphContextKey<string> buildingLastRequestedId = new("building.lastRequestedId");
		public static readonly GraphContextKey<string> questLastRequestedId = new("quest.lastRequestedId");
	}
}