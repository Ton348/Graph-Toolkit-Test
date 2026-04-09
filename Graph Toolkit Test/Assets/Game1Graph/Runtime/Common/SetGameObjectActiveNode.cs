using System;
using UnityEngine;

[Serializable]
public class SetGameObjectActiveNode : BusinessQuestNode
{
    public GameObject targetObject;
    public string siteId;
    public string visualId;
    public bool isActive;
}
