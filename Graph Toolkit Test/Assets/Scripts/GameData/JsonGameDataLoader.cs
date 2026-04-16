using System.IO;
using UnityEngine;

public class JsonGameDataLoader
{
    private readonly string m_rootPath;

    public JsonGameDataLoader(string rootPath)
    {
        this.m_rootPath = rootPath;
    }

    public QuestDatabaseData LoadQuests()
    {
        var data = Load<QuestDatabaseData>("quests.json");
        if (data != null)
        {
            Debug.Log($"[JsonGameDataLoader] Loaded quests: {data.quests?.Count ?? 0}");
        }
        return data;
    }

    public BuildingDatabaseData LoadBuildings()
    {
        var data = Load<BuildingDatabaseData>("buildings.json");
        if (data != null)
        {
            Debug.Log($"[JsonGameDataLoader] Loaded buildings: {data.buildings?.Count ?? 0}");
        }
        return data;
    }

    public EconomyConfigData LoadEconomy()
    {
        var data = Load<EconomyConfigData>("economy.json");
        if (data != null)
        {
            Debug.Log("[JsonGameDataLoader] Loaded economy config");
        }
        return data;
    }

    public LotDatabaseData LoadLots()
    {
        var data = Load<LotDatabaseData>("lots.json");
        if (data != null)
        {
            Debug.Log($"[JsonGameDataLoader] Loaded lots: {data.lots?.Count ?? 0}");
        }
        return data;
    }

    private T Load<T>(string fileName) where T : class
    {
        string path = Path.Combine(m_rootPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[JsonGameDataLoader] File not found: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError($"[JsonGameDataLoader] File is empty: {path}");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch
        {
            Debug.LogError($"[JsonGameDataLoader] Invalid JSON in: {path}");
            return null;
        }
    }
}
