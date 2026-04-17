using System;
using System.Collections.Generic;

namespace Prototype.Business.Runtime
{
	[Serializable]
	public class ProfileSnapshot
	{
		public int money;
		public int bargaining;
		public int speech;
		public int trading;
		public int speed;
		public int damage;
		public int health;
		public List<string> activeQuestIds = new();
		public List<string> completedQuestIds = new();
		public List<GraphCheckpointSnapshot> graphCheckpoints = new();
		public List<ConstructedSiteSnapshot> constructedSites = new();
		public List<BusinessInstanceSnapshot> businesses = new();
		public List<string> knownContacts = new();
	}

	[Serializable]
	public class GraphCheckpointSnapshot
	{
		public string graphId;
		public string checkpointId;
	}

	[Serializable]
	public class ConstructedSiteSnapshot
	{
		public string siteId;
		public string visualId;
		public bool isConstructed;
	}
}
