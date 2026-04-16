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
		public string locationId;
		public int size;
		public List<string> allowedBusinessTypes = new();
	}
}