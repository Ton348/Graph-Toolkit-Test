using Prototype.Business.NPC.Registry;

namespace Prototype.Business.Runtime
{
	public interface IBusinessDemandSource
	{
		bool TryConsumeService(BusinessInstanceSnapshot business, NPCServiceType service, out int price);
	}
}
