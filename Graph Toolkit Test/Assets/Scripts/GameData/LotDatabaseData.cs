using System;
using System.Collections.Generic;

namespace Sample.Runtime.GameData
{
	[Serializable]
	public class LotDatabaseData
	{
		public List<LotDefinitionData> lots = new();
	}
}