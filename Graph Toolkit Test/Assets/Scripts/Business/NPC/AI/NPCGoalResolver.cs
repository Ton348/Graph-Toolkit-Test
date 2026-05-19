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

			var t = needs.Thresholds;
			if (needs.Safety <= t.safetyCritical) return NPCGoalType.Flee;
			if (needs.Toilet >= t.toiletCritical) return NPCGoalType.GoToilet;
			if (needs.Hunger >= t.hungerCritical) return NPCGoalType.GoEat;
			if (needs.Thirst >= t.thirstCritical) return NPCGoalType.GoDrink;
			if (needs.Work >= t.workCritical) return NPCGoalType.GoWork;
			if (needs.Energy <= t.energyLow) return NPCGoalType.GoHome;
			if (needs.Fun <= t.funLow) return NPCGoalType.Wander;
			return NPCGoalType.Wander;
		}
	}
}
