using System;
using UnityEngine;

[Serializable]
public class SetGameObjectActiveNode : BusinessQuestNode
{
    public GameObject targetObject;
    public bool isActive;
    public string spawnKey;
}
