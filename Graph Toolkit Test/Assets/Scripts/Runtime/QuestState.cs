using Sample.Runtime.GameData;

namespace Sample.Runtime.Runtime
{
	public class QuestState
	{
		public QuestDefinitionData definition;
		public QuestStatus status;

		public QuestState(QuestDefinitionData definition)
		{
			this.definition = definition;
			status = QuestStatus.Inactive;
		}
	}
}