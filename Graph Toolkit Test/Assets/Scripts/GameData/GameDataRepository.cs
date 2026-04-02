using System.Collections.Generic;
using UnityEngine;

public class GameDataRepository
{
    private readonly Dictionary<string, QuestDefinitionData> questsById = new Dictionary<string, QuestDefinitionData>();
    private readonly Dictionary<string, BuildingDefinitionData> buildingsById = new Dictionary<string, BuildingDefinitionData>();
    private readonly Dictionary<string, LotDefinitionData> lotsById = new Dictionary<string, LotDefinitionData>();
    private readonly EconomyConfigData economy;

    public GameDataRepository(QuestDatabaseData questDatabase, BuildingDatabaseData buildingDatabase, EconomyConfigData economy, LotDatabaseData lotDatabase)
    {
        this.economy = economy ?? new EconomyConfigData();
        IndexQuests(questDatabase);
        IndexBuildings(buildingDatabase);
        IndexLots(lotDatabase);
    }

    public QuestDefinitionData GetQuestById(string id)
    {
        return !string.IsNullOrEmpty(id) && questsById.TryGetValue(id, out var quest) ? quest : null;
    }

    public BuildingDefinitionData GetBuildingById(string id)
    {
        return !string.IsNullOrEmpty(id) && buildingsById.TryGetValue(id, out var building) ? building : null;
    }

    public IEnumerable<QuestDefinitionData> GetAllQuests()
    {
        return questsById.Values;
    }

    public IEnumerable<BuildingDefinitionData> GetAllBuildings()
    {
        return buildingsById.Values;
    }

    public LotDefinitionData GetLotById(string id)
    {
        return !string.IsNullOrEmpty(id) && lotsById.TryGetValue(id, out var lot) ? lot : null;
    }

    public bool HasLot(string id)
    {
        return GetLotById(id) != null;
    }

    public IEnumerable<LotDefinitionData> GetAllLots()
    {
        return lotsById.Values;
    }

    public EconomyConfigData GetEconomy()
    {
        return economy;
    }

    private void IndexQuests(QuestDatabaseData questDatabase)
    {
        questsById.Clear();
        if (questDatabase == null || questDatabase.quests == null)
        {
            return;
        }

        foreach (var quest in questDatabase.quests)
        {
            if (quest == null || string.IsNullOrEmpty(quest.id))
            {
                continue;
            }

            if (!questsById.ContainsKey(quest.id))
            {
                questsById.Add(quest.id, quest);
            }
            else
            {
                Debug.LogWarning($"[GameDataRepository] Duplicate quest id: {quest.id}");
            }
        }
    }

    private void IndexBuildings(BuildingDatabaseData buildingDatabase)
    {
        buildingsById.Clear();
        if (buildingDatabase == null || buildingDatabase.buildings == null)
        {
            return;
        }

        foreach (var building in buildingDatabase.buildings)
        {
            if (building == null || string.IsNullOrEmpty(building.id))
            {
                continue;
            }

            if (!buildingsById.ContainsKey(building.id))
            {
                buildingsById.Add(building.id, building);
            }
            else
            {
                Debug.LogWarning($"[GameDataRepository] Duplicate building id: {building.id}");
            }
        }
    }

    private void IndexLots(LotDatabaseData lotDatabase)
    {
        lotsById.Clear();
        if (lotDatabase == null || lotDatabase.lots == null)
        {
            return;
        }

        foreach (var lot in lotDatabase.lots)
        {
            if (lot == null || string.IsNullOrEmpty(lot.id))
            {
                continue;
            }

            if (!lotsById.ContainsKey(lot.id))
            {
                lotsById.Add(lot.id, lot);
            }
            else
            {
                Debug.LogWarning($"[GameDataRepository] Duplicate lot id: {lot.id}");
            }
        }
    }

}
