using Sample.Runtime.GameData;

namespace Sample.Runtime.Runtime
{
	public class BuildingState
	{
		public int currentExpenses;
		public int currentIncome;
		public BuildingDefinitionData definition;
		public bool isOwned;
		public int level;

		public BuildingState(BuildingDefinitionData definition)
		{
			this.definition = definition;
			isOwned = false;
			level = 0;
			currentIncome = 0;
			currentExpenses = 0;
		}
	}
}