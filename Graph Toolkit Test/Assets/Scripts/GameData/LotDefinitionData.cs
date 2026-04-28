using System;
using System.Collections.Generic;

namespace Sample.Runtime.GameData
{
	[Serializable]
	public class LotDefinitionData
	{
		public string id;
		public string displayName;
		public int rentPerDay;
		public List<string> allowedBusinessTypes = new();
	}
}
