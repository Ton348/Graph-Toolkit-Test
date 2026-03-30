using System;
using System.Collections.Generic;

[Serializable]
public class ProfileSnapshot
{
    public int Money;
    public int Bargaining;
    public int Speech;
    public int Speed;
    public int Damage;
    public int Health;
    public List<string> ActiveQuestIds = new List<string>();
    public List<string> CompletedQuestIds = new List<string>();
    public List<string> OwnedBuildingIds = new List<string>();
    public List<BuildingStateSnapshot> BuildingStates = new List<BuildingStateSnapshot>();
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
