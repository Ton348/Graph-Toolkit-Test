using System;
using System.Collections.Generic;

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
    public List<string> activeQuestIds = new List<string>();
    public List<string> completedQuestIds = new List<string>();
    public List<string> ownedBuildingIds = new List<string>();
    public List<BuildingStateSnapshot> buildingStates = new List<BuildingStateSnapshot>();
    public List<GraphCheckpointSnapshot> graphCheckpoints = new List<GraphCheckpointSnapshot>();
    public List<ConstructedSiteSnapshot> constructedSites = new List<ConstructedSiteSnapshot>();
    public List<BusinessInstanceSnapshot> businesses = new List<BusinessInstanceSnapshot>();
    public List<string> knownContacts = new List<string>();
}

[Serializable]
public class BuildingStateSnapshot
{
    public string id;
    public bool owned;
    public int level;
    public int currentIncome;
    public int currentExpenses;
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
