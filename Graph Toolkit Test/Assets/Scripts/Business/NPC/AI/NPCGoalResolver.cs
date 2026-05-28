using Prototype.Business.NPC.Needs;

namespace Prototype.Business.NPC.AI
{
	public sealed class NPCGoalResolver
	{
		public NPCGoalType ResolveGoal(NPCNeedsComponent needs)
		{
			if (needs == null)
			{
				return NPCGoalType.Idle;
			}

			if (needs.Safety <= 35f) return NPCGoalType.Flee;
			if (needs.Toilet >= 80f) return NPCGoalType.GoToilet;
			if (needs.Hunger >= 75f) return NPCGoalType.GoEat;
			if (needs.Thirst >= 75f) return NPCGoalType.GoDrink;
			if (needs.Work >= 70f) return NPCGoalType.GoWork;
			if (needs.Energy <= 30f) return NPCGoalType.GoHome;
			if (needs.Fun <= 30f) return NPCGoalType.Wander;
			return NPCGoalType.Wander;
		}
	}
}
