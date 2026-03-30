using System;
using System.Collections.Generic;

[Serializable]
public class ProfileSnapshot
{
    public int Money;
    public List<string> ActiveQuestIds = new List<string>();
    public List<string> CompletedQuestIds = new List<string>();
    public List<string> OwnedBuildingIds = new List<string>();
}
