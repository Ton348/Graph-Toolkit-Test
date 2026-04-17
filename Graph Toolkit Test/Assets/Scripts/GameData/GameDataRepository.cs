using System.Collections.Generic;
using UnityEngine;

namespace Sample.Runtime.GameData
{
	public class GameDataRepository
	{
		private readonly Dictionary<string, BuildingDefinitionData> m_buildingsById = new();
		private readonly EconomyConfigData m_economy;
		private readonly Dictionary<string, LotDefinitionData> m_lotsById = new();
		private readonly Dictionary<string, QuestDefinitionData> m_questsById = new();

		public GameDataRepository(
			QuestDatabaseData questDatabase,
			BuildingDatabaseData buildingDatabase,
			EconomyConfigData economy,
			LotDatabaseData lotDatabase)
		{
			m_economy = economy ?? new EconomyConfigData();
			IndexQuests(questDatabase);
			IndexBuildings(buildingDatabase);
			IndexLots(lotDatabase);
		}

		public QuestDefinitionData GetQuestById(string id)
		{
			return !string.IsNullOrEmpty(id) && m_questsById.TryGetValue(id, out QuestDefinitionData quest) ? quest : null;
		}

		public BuildingDefinitionData GetBuildingById(string id)
		{
			return !string.IsNullOrEmpty(id) && m_buildingsById.TryGetValue(id, out BuildingDefinitionData building)
				? building
				: null;
		}

		public IEnumerable<QuestDefinitionData> GetAllQuests()
		{
			return m_questsById.Values;
		}

		public IEnumerable<BuildingDefinitionData> GetAllBuildings()
		{
			return m_buildingsById.Values;
		}

		public LotDefinitionData GetLotById(string id)
		{
			return !string.IsNullOrEmpty(id) && m_lotsById.TryGetValue(id, out LotDefinitionData lot) ? lot : null;
		}

		public bool HasLot(string id)
		{
			return GetLotById(id) != null;
		}

		public IEnumerable<LotDefinitionData> GetAllLots()
		{
			return m_lotsById.Values;
		}

		public EconomyConfigData GetEconomy()
		{
			return m_economy;
		}

		private void IndexQuests(QuestDatabaseData questDatabase)
		{
			m_questsById.Clear();
			if (questDatabase == null || questDatabase.quests == null)
			{
				return;
			}

			foreach (QuestDefinitionData quest in questDatabase.quests)
			{
				if (quest == null || string.IsNullOrEmpty(quest.id))
				{
					continue;
				}

				if (!m_questsById.ContainsKey(quest.id))
				{
					m_questsById.Add(quest.id, quest);
				}
				else
				{
					Debug.LogWarning($"[GameDataRepository] Duplicate quest id: {quest.id}");
				}
			}
		}

		private void IndexBuildings(BuildingDatabaseData buildingDatabase)
		{
			m_buildingsById.Clear();
			if (buildingDatabase == null || buildingDatabase.buildings == null)
			{
				return;
			}

			foreach (BuildingDefinitionData building in buildingDatabase.buildings)
			{
				if (building == null || string.IsNullOrEmpty(building.id))
				{
					continue;
				}

				if (!m_buildingsById.ContainsKey(building.id))
				{
					m_buildingsById.Add(building.id, building);
				}
				else
				{
					Debug.LogWarning($"[GameDataRepository] Duplicate building id: {building.id}");
				}
			}
		}

		private void IndexLots(LotDatabaseData lotDatabase)
		{
			m_lotsById.Clear();
			if (lotDatabase == null || lotDatabase.lots == null)
			{
				return;
			}

			foreach (LotDefinitionData lot in lotDatabase.lots)
			{
				if (lot == null || string.IsNullOrEmpty(lot.id))
				{
					continue;
				}

				if (!m_lotsById.ContainsKey(lot.id))
				{
					m_lotsById.Add(lot.id, lot);
				}
				else
				{
					Debug.LogWarning($"[GameDataRepository] Duplicate lot id: {lot.id}");
				}
			}
		}
	}
}