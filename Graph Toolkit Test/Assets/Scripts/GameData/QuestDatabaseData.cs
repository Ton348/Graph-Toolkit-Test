using System;
using System.Collections.Generic;

namespace Sample.Runtime.GameData
{
	[Serializable]
	public class QuestDatabaseData
	{
		public List<QuestDefinitionData> quests = new();
	}
}