using System;
using System.IO;
using UnityEngine;

namespace Prototype.Business.Data
{
	public sealed class JsonBusinessDataLoader
	{
		private readonly string m_rootPath;
		private BusinessPeopleDatabaseData m_peopleCache;

		public JsonBusinessDataLoader(string rootPath)
		{
			m_rootPath = rootPath;
		}

		public BusinessTypeDatabaseData LoadBusinessTypes()
		{
			return Load<BusinessTypeDatabaseData>("business_types.json", "business types");
		}

		public BusinessModuleDatabaseData LoadBusinessModules()
		{
			return Load<BusinessModuleDatabaseData>("business_modules.json", "business modules");
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
					throughputPerHour = person.throughputPerHour
				});
			}

			return db;
		}

		public CustomerBehaviorDatabaseData LoadCustomerBehaviors()
		{
			return Load<CustomerBehaviorDatabaseData>("customer_behavior.json", "customer behaviors");
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

			m_peopleCache = Load<BusinessPeopleDatabaseData>("people.json", "business people") ?? new BusinessPeopleDatabaseData();
			return m_peopleCache;
		}

	}
}
