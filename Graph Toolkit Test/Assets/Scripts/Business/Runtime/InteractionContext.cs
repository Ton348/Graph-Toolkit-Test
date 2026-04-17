using Prototype.Business.NPC;

namespace Prototype.Business.Runtime
{
	public enum InteractionContextType
	{
		Normal,
		Steal
	}

	public class InteractionContext
	{
		public InteractionContextType contextType;
		public Npcmanager sourceNpc;
	}
}