using Prototype.Business.Data;
using Sample.Runtime.GameData;
using UnityEngine;

namespace Prototype.Business.Runtime
{
	public readonly struct BusinessRuntimeStats
	{
		public readonly int RentPerDay;
		public readonly int StorageCapacity;
		public readonly int ShelfCapacity;
		public readonly int StorageFreeSpace;
		public readonly int ShelfFreeSpace;

		public BusinessRuntimeStats(
			int rentPerDay,
			int storageCapacity,
			int shelfCapacity,
			int storageFreeSpace,
			int shelfFreeSpace)
		{
			RentPerDay = rentPerDay;
			StorageCapacity = storageCapacity;
			ShelfCapacity = shelfCapacity;
			StorageFreeSpace = storageFreeSpace;
			ShelfFreeSpace = shelfFreeSpace;
		}
	}

	public static class BusinessRuntimeStatsCalculator
	{
		public static BusinessRuntimeStats Calculate(
			BusinessInstanceSnapshot business,
			GameDataRepository gameData,
			BusinessDefinitionsRepository definitions)
		{
			if (business == null)
			{
				return new BusinessRuntimeStats(0, 0, 0, 0, 0);
			}

			int rentPerDay = ResolveRentPerDay(business, gameData);
			int storageCapacity = ResolveStorageCapacity(business, definitions);
			int shelfCapacity = ResolveShelfCapacity(business, definitions);
			int storageFreeSpace = Mathf.Max(0, storageCapacity - Mathf.Max(0, business.storageStock));
			int shelfFreeSpace = Mathf.Max(0, shelfCapacity - Mathf.Max(0, business.shelfStock));

			return new BusinessRuntimeStats(
				rentPerDay,
				storageCapacity,
				shelfCapacity,
				storageFreeSpace,
				shelfFreeSpace);
		}

		public static int ResolveRentPerDay(BusinessInstanceSnapshot business, GameDataRepository gameData)
		{
			if (business == null || gameData == null || string.IsNullOrWhiteSpace(business.lotId))
			{
				return 0;
			}

			LotDefinitionData lot = gameData.GetLotById(business.lotId);
			return lot != null ? Mathf.Max(0, lot.rentPerDay) : 0;
		}

		public static int ResolveStorageCapacity(BusinessInstanceSnapshot business, BusinessDefinitionsRepository definitions)
		{
			if (business == null || definitions == null || string.IsNullOrWhiteSpace(business.storageItemId))
			{
				return 0;
			}

			TraderItemDefinitionData item = definitions.GetTraderItem(business.storageItemId);
			return item != null ? Mathf.Max(0, item.storageCapacity) : 0;
		}

		public static int ResolveShelfCapacity(BusinessInstanceSnapshot business, BusinessDefinitionsRepository definitions)
		{
			if (business == null || definitions == null || string.IsNullOrWhiteSpace(business.shelfItemId))
			{
				return 0;
			}

			TraderItemDefinitionData item = definitions.GetTraderItem(business.shelfItemId);
			return item != null ? Mathf.Max(0, item.shelfCapacity) : 0;
		}
	}
}
