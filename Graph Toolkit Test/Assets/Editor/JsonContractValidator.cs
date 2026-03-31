using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class JsonContractValidator
{
    private const string MenuPath = "Tools/Game Data/Validate Client Server JSON";

    [MenuItem(MenuPath)]
    public static void Validate()
    {
        int ok = 0;
        int warn = 0;
        int error = 0;

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        string clientRoot = Path.Combine(Application.dataPath, "StreamingAssets", "GameData");
        string serverRoot = Path.Combine(projectRoot, "server", "data");

        LogOk($"Client root: {clientRoot}", ref ok);
        LogOk($"Server root: {serverRoot}", ref ok);

        string clientQuestsPath = Path.Combine(clientRoot, "quests.json");
        string clientBuildingsPath = Path.Combine(clientRoot, "buildings.json");
        string clientEconomyPath = Path.Combine(clientRoot, "economy.json");
        string serverQuestsPath = Path.Combine(serverRoot, "quests.json");
        string serverBuildingsPath = Path.Combine(serverRoot, "buildings.json");
        string serverEconomyPath = Path.Combine(serverRoot, "economy.json");

        var clientQuestsRaw = ReadText(clientQuestsPath, "client quests.json", ref ok, ref warn, ref error);
        var clientBuildingsRaw = ReadText(clientBuildingsPath, "client buildings.json", ref ok, ref warn, ref error);
        var clientEconomyRaw = ReadText(clientEconomyPath, "client economy.json", ref ok, ref warn, ref error);
        var serverQuestsRaw = ReadText(serverQuestsPath, "server quests.json", ref ok, ref warn, ref error);
        var serverBuildingsRaw = ReadText(serverBuildingsPath, "server buildings.json", ref ok, ref warn, ref error);
        var serverEconomyRaw = ReadText(serverEconomyPath, "server economy.json", ref ok, ref warn, ref error);

        var clientQuestDb = ParseJson<QuestDatabaseData>(clientQuestsRaw, "client quests.json", ref ok, ref warn, ref error);
        var clientBuildingDb = ParseJson<BuildingDatabaseData>(clientBuildingsRaw, "client buildings.json", ref ok, ref warn, ref error);
        var clientEconomy = ParseJson<EconomyConfigData>(clientEconomyRaw, "client economy.json", ref ok, ref warn, ref error);
        var serverQuestDb = ParseJson<QuestDatabaseData>(serverQuestsRaw, "server quests.json", ref ok, ref warn, ref error);
        var serverBuildingDb = ParseJson<BuildingDatabaseData>(serverBuildingsRaw, "server buildings.json", ref ok, ref warn, ref error);
        var serverEconomy = ParseJson<EconomyConfigData>(serverEconomyRaw, "server economy.json", ref ok, ref warn, ref error);

        var clientQuestIds = ValidateQuestDb(clientQuestDb, "client", ref ok, ref warn, ref error);
        var serverQuestIds = ValidateQuestDb(serverQuestDb, "server", ref ok, ref warn, ref error);
        var clientBuildingIds = ValidateBuildingDb(clientBuildingDb, "client", ref ok, ref warn, ref error);
        var serverBuildingIds = ValidateBuildingDb(serverBuildingDb, "server", ref ok, ref warn, ref error);

        CompareIdSets("quests", clientQuestIds, serverQuestIds, ref ok, ref warn, ref error);
        CompareIdSets("buildings", clientBuildingIds, serverBuildingIds, ref ok, ref warn, ref error);

        CompareQuestFields(clientQuestDb, serverQuestDb, clientQuestsRaw, serverQuestsRaw, ref ok, ref warn, ref error);
        CompareBuildingFields(clientBuildingDb, serverBuildingDb, clientBuildingsRaw, serverBuildingsRaw, ref ok, ref warn, ref error);
        CompareEconomy(clientEconomy, serverEconomy, ref ok, ref warn, ref error);

        if (error == 0)
        {
            LogOk($"[JSON VALIDATION] Completed successfully. No critical mismatches found. OK={ok} WARN={warn} ERROR={error}", ref ok);
        }
        else
        {
            LogError($"[JSON VALIDATION] Completed with errors. OK={ok} WARN={warn} ERROR={error}", ref error);
        }
    }

    private static string ReadText(string path, string label, ref int ok, ref int warn, ref int error)
    {
        if (!File.Exists(path))
        {
            LogError($"[JSON VALIDATION][ERROR] Missing file: {label} ({path})", ref error);
            return null;
        }

        try
        {
            LogOk($"[JSON VALIDATION][OK] Loaded {label}", ref ok);
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            LogError($"[JSON VALIDATION][ERROR] Failed to read {label}: {ex.Message}", ref error);
            return null;
        }
    }

    private static T ParseJson<T>(string raw, string label, ref int ok, ref int warn, ref int error) where T : class
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            LogError($"[JSON VALIDATION][ERROR] Empty JSON: {label}", ref error);
            return null;
        }

        try
        {
            var data = JsonUtility.FromJson<T>(raw);
            if (data == null)
            {
                LogError($"[JSON VALIDATION][ERROR] Failed to parse {label}", ref error);
            }
            else
            {
                LogOk($"[JSON VALIDATION][OK] Parsed {label}", ref ok);
            }
            return data;
        }
        catch (Exception ex)
        {
            LogError($"[JSON VALIDATION][ERROR] Invalid JSON in {label}: {ex.Message}", ref error);
            return null;
        }
    }

    private static HashSet<string> ValidateQuestDb(QuestDatabaseData db, string scope, ref int ok, ref int warn, ref int error)
    {
        var ids = new HashSet<string>();
        if (db == null || db.quests == null)
        {
            LogError($"[JSON VALIDATION][ERROR] {scope} quests.json missing quests array", ref error);
            return ids;
        }

        for (int i = 0; i < db.quests.Count; i++)
        {
            var q = db.quests[i];
            if (q == null || string.IsNullOrWhiteSpace(q.id))
            {
                LogError($"[JSON VALIDATION][ERROR] {scope} quest at index {i} missing id", ref error);
                continue;
            }
            if (!ids.Add(q.id))
            {
                LogError($"[JSON VALIDATION][ERROR] {scope} duplicate quest id: {q.id}", ref error);
            }
        }

        LogOk($"[JSON VALIDATION][OK] {scope} quests count: {ids.Count}", ref ok);
        return ids;
    }

    private static HashSet<string> ValidateBuildingDb(BuildingDatabaseData db, string scope, ref int ok, ref int warn, ref int error)
    {
        var ids = new HashSet<string>();
        if (db == null || db.buildings == null)
        {
            LogError($"[JSON VALIDATION][ERROR] {scope} buildings.json missing buildings array", ref error);
            return ids;
        }

        for (int i = 0; i < db.buildings.Count; i++)
        {
            var b = db.buildings[i];
            if (b == null || string.IsNullOrWhiteSpace(b.id))
            {
                LogError($"[JSON VALIDATION][ERROR] {scope} building at index {i} missing id", ref error);
                continue;
            }
            if (!ids.Add(b.id))
            {
                LogError($"[JSON VALIDATION][ERROR] {scope} duplicate building id: {b.id}", ref error);
            }
        }

        LogOk($"[JSON VALIDATION][OK] {scope} buildings count: {ids.Count}", ref ok);
        return ids;
    }

    private static void CompareIdSets(string label, HashSet<string> client, HashSet<string> server, ref int ok, ref int warn, ref int error)
    {
        foreach (var id in client.Except(server))
        {
            LogError($"[JSON VALIDATION][ERROR] Missing {label} id on server: {id}", ref error);
        }
        foreach (var id in server.Except(client))
        {
            LogError($"[JSON VALIDATION][ERROR] Missing {label} id on client: {id}", ref error);
        }

        LogOk($"[JSON VALIDATION][OK] {label} id sets compared", ref ok);
    }

    private static void CompareQuestFields(QuestDatabaseData client, QuestDatabaseData server, string clientRaw, string serverRaw, ref int ok, ref int warn, ref int error)
    {
        if (client == null || server == null) return;

        var serverById = server.quests?.Where(q => q != null && !string.IsNullOrWhiteSpace(q.id))
            .ToDictionary(q => q.id, q => q) ?? new Dictionary<string, QuestDefinitionData>();

        foreach (var quest in client.quests ?? new List<QuestDefinitionData>())
        {
            if (quest == null || string.IsNullOrWhiteSpace(quest.id)) continue;
            if (!serverById.TryGetValue(quest.id, out var serverQuest))
            {
                continue;
            }

            if (!HasFieldForId(clientRaw, quest.id, "rewardMoney"))
            {
                LogWarn($"[JSON VALIDATION][WARN] client quest {quest.id} missing field rewardMoney", ref warn);
            }
            if (!HasFieldForId(serverRaw, quest.id, "rewardMoney"))
            {
                LogWarn($"[JSON VALIDATION][WARN] server quest {quest.id} missing field rewardMoney", ref warn);
            }

            if (quest.rewardMoney != serverQuest.rewardMoney)
            {
                LogWarn($"[JSON VALIDATION][WARN] quest {quest.id} rewardMoney mismatch client={quest.rewardMoney} server={serverQuest.rewardMoney}", ref warn);
            }
        }

        LogOk($"[JSON VALIDATION][OK] quest fields compared", ref ok);
    }

    private static void CompareBuildingFields(BuildingDatabaseData client, BuildingDatabaseData server, string clientRaw, string serverRaw, ref int ok, ref int warn, ref int error)
    {
        if (client == null || server == null) return;

        var serverById = server.buildings?.Where(b => b != null && !string.IsNullOrWhiteSpace(b.id))
            .ToDictionary(b => b.id, b => b) ?? new Dictionary<string, BuildingDefinitionData>();

        foreach (var building in client.buildings ?? new List<BuildingDefinitionData>())
        {
            if (building == null || string.IsNullOrWhiteSpace(building.id)) continue;
            if (!serverById.TryGetValue(building.id, out var serverBuilding))
            {
                continue;
            }

            if (!HasFieldForId(clientRaw, building.id, "purchaseCost"))
            {
                LogWarn($"[JSON VALIDATION][WARN] client building {building.id} missing field purchaseCost", ref warn);
            }
            if (!HasFieldForId(serverRaw, building.id, "purchaseCost"))
            {
                LogWarn($"[JSON VALIDATION][WARN] server building {building.id} missing field purchaseCost", ref warn);
            }

            if (building.purchaseCost != serverBuilding.purchaseCost)
            {
                LogWarn($"[JSON VALIDATION][WARN] building {building.id} purchaseCost mismatch client={building.purchaseCost} server={serverBuilding.purchaseCost}", ref warn);
            }
        }

        LogOk($"[JSON VALIDATION][OK] building fields compared", ref ok);
    }

    private static void CompareEconomy(EconomyConfigData client, EconomyConfigData server, ref int ok, ref int warn, ref int error)
    {
        if (client == null || server == null)
        {
            LogWarn("[JSON VALIDATION][WARN] economy.json not compared (missing)", ref warn);
            return;
        }

        if (client.startMoney != server.startMoney)
        {
            LogWarn($"[JSON VALIDATION][WARN] economy.startMoney mismatch client={client.startMoney} server={server.startMoney}", ref warn);
        }
        else
        {
            LogOk("[JSON VALIDATION][OK] economy.startMoney matches", ref ok);
        }
    }

    private static bool HasFieldForId(string raw, string id, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        var pattern = "\\{[^{}]*\"id\"\\s*:\\s*\"" + Regex.Escape(id) + "\"[^{}]*\\}";
        var match = Regex.Match(raw, pattern);
        if (!match.Success)
        {
            return false;
        }

        return match.Value.Contains($"\"{fieldName}\"");
    }

    private static void LogOk(string message, ref int ok)
    {
        Debug.Log(message);
        ok++;
    }

    private static void LogWarn(string message, ref int warn)
    {
        Debug.LogWarning(message);
        warn++;
    }

    private static void LogError(string message, ref int error)
    {
        Debug.LogError(message);
        error++;
    }
}
