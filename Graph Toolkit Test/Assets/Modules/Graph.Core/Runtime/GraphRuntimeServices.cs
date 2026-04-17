namespace Graph.Core.Runtime
{
	public sealed class GraphRuntimeServices : IGraphRuntimeServices
	{
		public GraphRuntimeServices(
			IGraphDialogueService dialogueService,
			IGraphChoiceService choiceService,
			IGraphMapMarkerService mapMarkerService,
			IGraphCutsceneService cutsceneService,
			IGraphCheckpointService checkpointService,
			IGraphQuestService questService)
		{
			DialogueService = dialogueService;
			ChoiceService = choiceService;
			MapMarkerService = mapMarkerService;
			CutsceneService = cutsceneService;
			CheckpointService = checkpointService;
			QuestService = questService;
		}

		public IGraphDialogueService DialogueService { get; }
		public IGraphChoiceService ChoiceService { get; }
		public IGraphMapMarkerService MapMarkerService { get; }
		public IGraphCutsceneService CutsceneService { get; }
		public IGraphCheckpointService CheckpointService { get; }
		public IGraphQuestService QuestService { get; }
	}
}