using System;
using System.Collections.Generic;

namespace Sample.Runtime.GameData
{
	[Serializable]
	public class BuildingDatabaseData
	{
		public List<BuildingDefinitionData> buildings = new();
	}
}