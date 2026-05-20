using System;
using System.IO;
using UnityEngine;

namespace Prototype.Business.Data
{
	public sealed class JsonBusinessDataLoader
	{
		private readonly string m_rootPath;
		private BusinessPeopleDatabaseData m_peopleCache;
		private TraderItemDatabaseData m_itemsCache;
		private TraderDatabaseData m_tradersCache;

		public JsonBusinessDataLoader(string rootPath)
		{
			m_rootPath = rootPath;
		}

		public BusinessTypeDatabaseData LoadBusinessTypes()
		{
			return Load<BusinessTypeDatabaseData>("business_types.json", "business types");
		}

		public SupplierDatabaseData LoadSuppliers()
		{
			BusinessPeopleDatabaseData peopleData = LoadPeople();
			var db = new SupplierDatabaseData();
			if (peopleData?.people == null)
			{
				return db;
			}

			foreach (BusinessPersonDefinitionData person in peopleData.people)
			{
				if (person == null || string.IsNullOrWhiteSpace(person.contactId) || person.supplierConfig == null)
				{
					continue;
				}

				db.suppliers.Add(new SupplierDefinitionData
				{
					id = person.contactId,
					displayName = string.IsNullOrWhiteSpace(person.displayName) ? person.contactId : person.displayName,
					productType = person.supplierConfig.productType,
					unitBuyPrice = person.supplierConfig.unitBuyPrice,
					minDeliveryAmount = person.supplierConfig.minDeliveryAmount,
					maxDeliveryAmount = person.supplierConfig.maxDeliveryAmount
				});
			}

			return db;
		}

		public StaffRoleDatabaseData LoadStaffRoles()
		{
			return new StaffRoleDatabaseData();
		}

		public StaffContactDatabaseData LoadStaffContacts()
		{
			BusinessPeopleDatabaseData peopleData = LoadPeople();
			var db = new StaffContactDatabaseData();
			if (peopleData?.people == null)
			{
				return db;
			}

			foreach (BusinessPersonDefinitionData person in peopleData.people)
			{
				if (person == null || string.IsNullOrWhiteSpace(person.contactId))
				{
					continue;
				}

				db.contacts.Add(new StaffContactDefinitionData
				{
					id = person.contactId,
					displayName = string.IsNullOrWhiteSpace(person.displayName) ? person.contactId : person.displayName,
					salaryPerDay = person.salaryPerDay,
					throughputPerHour = person.throughputPerHour,
					carryCapacity = person.carryCapacity,
					checkoutSecondsPerItem = person.checkoutSecondsPerItem
				});
			}

			return db;
		}

		public BusinessPeopleDatabaseData LoadPeopleData()
		{
			return LoadPeople();
		}

		public PriceDemandDatabaseData LoadPizzeriaDemand()
		{
			return Load<PriceDemandDatabaseData>("pizzeria_demand.json", "pizzeria demand");
		}

		public TraderDatabaseData LoadTraders()
		{
			if (m_tradersCache != null)
			{
				return m_tradersCache;
			}

			m_tradersCache = LoadTraderFolder("Traders", "traders");
			if (m_tradersCache.traders.Count == 0)
			{
				LegacyTraderDatabaseData legacy = Load<LegacyTraderDatabaseData>("traders.json", "traders");
				if (legacy?.traders != null)
				{
					foreach (LegacyTraderDefinitionData trader in legacy.traders)
					{
						if (trader == null)
						{
							continue;
						}

						var converted = new TraderDefinitionData
						{
							id = trader.id,
							name = trader.name
						};

						if (trader.items != null)
						{
							foreach (TraderItemDefinitionData item in trader.items)
							{
								if (item != null && !string.IsNullOrWhiteSpace(item.id))
								{
									converted.itemIds.Add(item.id);
								}
							}
						}

						m_tradersCache.traders.Add(converted);
					}
				}
			}

			return m_tradersCache;
		}

		public TraderItemDatabaseData LoadTraderItems()
		{
			if (m_itemsCache != null)
			{
				return m_itemsCache;
			}

			m_itemsCache = LoadItemFolder("Items", "trader items");
			if (m_itemsCache.items.Count == 0)
			{
				LegacyTraderDatabaseData legacy = Load<LegacyTraderDatabaseData>("traders.json", "traders");
				if (legacy?.traders != null)
				{
					foreach (LegacyTraderDefinitionData trader in legacy.traders)
					{
						if (trader?.items == null)
						{
							continue;
						}

						foreach (TraderItemDefinitionData item in trader.items)
						{
							if (item != null)
							{
								m_itemsCache.items.Add(item);
							}
						}
					}
				}
			}

			return m_itemsCache;
		}

		private T Load<T>(string fileName, string label) where T : class
		{
			string path = Path.Combine(m_rootPath, fileName);
			if (!File.Exists(path))
			{
				Debug.LogError($"[JsonBusinessDataLoader] Missing {label} file: {path}");
				return null;
			}

			try
			{
				string json = File.ReadAllText(path);
				if (string.IsNullOrWhiteSpace(json))
				{
					Debug.LogError($"[JsonBusinessDataLoader] Empty JSON for {label}: {path}");
					return null;
				}

				var data = JsonUtility.FromJson<T>(json);
				if (data == null)
				{
					Debug.LogError($"[JsonBusinessDataLoader] Failed to parse {label}: {path}");
					return null;
				}

				Debug.Log($"[JsonBusinessDataLoader] Loaded {label}");
				return data;
			}
			catch (SystemException ex)
			{
				Debug.LogError($"[JsonBusinessDataLoader] Error reading {label}: {ex.Message}");
				return null;
			}
		}

		private BusinessPeopleDatabaseData LoadPeople()
		{
			if (m_peopleCache != null)
			{
				return m_peopleCache;
			}

			m_peopleCache = LoadPeopleFolder("People", "business people");
			if (m_peopleCache.people.Count == 0)
			{
				m_peopleCache = Load<BusinessPeopleDatabaseData>("people.json", "business people") ??
				                new BusinessPeopleDatabaseData();
			}
			return m_peopleCache;
		}

		private BusinessPeopleDatabaseData LoadPeopleFolder(string relativeFolder, string label)
		{
			string folderPath = Path.Combine(m_rootPath, relativeFolder);
			var result = new BusinessPeopleDatabaseData();
			if (!Directory.Exists(folderPath))
			{
				return result;
			}

			string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				BusinessPersonDefinitionData person = LoadAbsolute<BusinessPersonDefinitionData>(files[i], label);
				if (person != null)
				{
					result.people.Add(person);
				}
			}

			return result;
		}

		private TraderItemDatabaseData LoadItemFolder(string relativeFolder, string label)
		{
			string folderPath = Path.Combine(m_rootPath, relativeFolder);
			var result = new TraderItemDatabaseData();
			if (!Directory.Exists(folderPath))
			{
				return result;
			}

			string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				TraderItemDefinitionData row = LoadAbsolute<TraderItemDefinitionData>(files[i], label);
				if (row != null)
				{
					result.items.Add(row);
				}
			}

			return result;
		}

		private TraderDatabaseData LoadTraderFolder(string relativeFolder, string label)
		{
			string folderPath = Path.Combine(m_rootPath, relativeFolder);
			var result = new TraderDatabaseData();
			if (!Directory.Exists(folderPath))
			{
				return result;
			}

			string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				TraderDefinitionData row = LoadAbsolute<TraderDefinitionData>(files[i], label);
				if (row != null)
				{
					result.traders.Add(row);
				}
			}

			return result;
		}

		private T LoadAbsolute<T>(string path, string label) where T : class
		{
			if (!File.Exists(path))
			{
				Debug.LogError($"[JsonBusinessDataLoader] Missing {label} file: {path}");
				return null;
			}

			try
			{
				string json = File.ReadAllText(path);
				if (string.IsNullOrWhiteSpace(json))
				{
					Debug.LogError($"[JsonBusinessDataLoader] Empty JSON for {label}: {path}");
					return null;
				}

				var data = JsonUtility.FromJson<T>(json);
				if (data == null)
				{
					Debug.LogError($"[JsonBusinessDataLoader] Failed to parse {label}: {path}");
					return null;
				}

				return data;
			}
			catch (SystemException ex)
			{
				Debug.LogError($"[JsonBusinessDataLoader] Error reading {label}: {ex.Message}");
				return null;
			}
		}

		[Serializable]
		private sealed class LegacyTraderDefinitionData
		{
			public string id;
			public string name;
			public TraderItemDefinitionData[] items;
		}

		[Serializable]
		private sealed class LegacyTraderDatabaseData
		{
			public LegacyTraderDefinitionData[] traders;
		}
	}
}
