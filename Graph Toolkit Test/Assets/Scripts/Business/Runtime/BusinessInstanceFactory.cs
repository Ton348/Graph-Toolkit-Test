using Prototype.Business.Data;
using Prototype.Business.NPC.Registry;
using Sample.Runtime.GameData;
using System.Collections.Generic;

namespace Prototype.Business.Runtime
{
	public static class BusinessInstanceFactory
	{
		public static BusinessInstanceSnapshot CreateBusinessInstance(
			LotDefinitionData lot,
			string businessTypeId,
			BusinessDefinitionsRepository definitions)
		{
			var business = new BusinessInstanceSnapshot
			{
				instanceId = $"local_{System.Guid.NewGuid():N}",
				lotId = lot != null ? lot.id : null,
				businessTypeId = string.IsNullOrWhiteSpace(businessTypeId) ? string.Empty : businessTypeId.Trim(),
				isOpen = false
			};

			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return business;
			}

			return business.ApplyBusinessTypeTemplate(businessTypeId, definitions);
		}

		public static BusinessInstanceSnapshot ApplyBusinessTypeTemplate(
			this BusinessInstanceSnapshot business,
			string businessTypeId,
			BusinessDefinitionsRepository definitions)
		{
			if (business == null)
			{
				return null;
			}

			BusinessTypeDefinitionData typeDef = !string.IsNullOrWhiteSpace(businessTypeId)
				? definitions?.GetBusinessType(businessTypeId)
				: null;
			BusinessInstanceTemplateData template = typeDef?.instanceTemplate;

			business.businessTypeId = string.IsNullOrWhiteSpace(businessTypeId) ? null : businessTypeId.Trim();
			business.isOpen = false;
			business.storageItemId = template != null ? template.storageItemId : null;
			business.cashDeskItemId = template != null ? template.cashDeskItemId : null;
			business.shelfItemId = template != null ? template.shelfItemId : null;
			business.services = BuildServices(typeDef);
			business.hiredCashierContactId = template != null ? template.hiredCashierContactId : null;
			business.hiredMerchContactId = template != null ? template.hiredMerchContactId : null;
			business.hiredLogistContactId = template != null ? template.hiredLogistContactId : null;
			return business;
		}

		private static List<string> BuildServices(BusinessTypeDefinitionData typeDef)
		{
			var services = new List<string>();
			if (typeDef?.services == null)
			{
				return services;
			}

			for (int i = 0; i < typeDef.services.Count; i++)
			{
				services.Add(typeDef.services[i].ToString());
			}

			return services;
		}
	}
}
