using System.Collections.Generic;

namespace Sample.Runtime.Runtime
{
	public class GameRuntimeState
	{
		public List<BuildingState> buildings;
		public PlayerProfileState player;
		public List<QuestState> quests;
	}
}