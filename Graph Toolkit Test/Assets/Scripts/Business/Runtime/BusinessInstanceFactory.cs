using Prototype.Business.Data;
using Sample.Runtime.GameData;

namespace Prototype.Business.Runtime
{
	public static class BusinessInstanceFactory
	{
		public static BusinessInstanceSnapshot CreateBusinessInstance(LotDefinitionData lot, string businessTypeId)
		{
			var business = new BusinessInstanceSnapshot
			{
				instanceId = $"local_{System.Guid.NewGuid():N}",
				lotId = lot != null ? lot.id : null,
				businessTypeId = string.IsNullOrWhiteSpace(businessTypeId) ? string.Empty : businessTypeId.Trim(),
				isOpen = false,
				rentPerDay = lot != null && lot.rentPerDay > 0 ? lot.rentPerDay : 0
			};

			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return business;
			}

			return business.ApplyBusinessTypeTemplate(businessTypeId, null);
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
			business.hiredCashierContactId = template != null ? template.hiredCashierContactId : null;
			business.hiredMerchContactId = template != null ? template.hiredMerchContactId : null;
			business.hiredLogistContactId = template != null ? template.hiredLogistContactId : null;
			return business;
		}
	}
}
