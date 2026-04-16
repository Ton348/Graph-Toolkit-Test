namespace GraphCore.Runtime
{
	public interface IGraphRuntimeServices
	{
		IGraphDialogueService DialogueService { get; }
		IGraphChoiceService ChoiceService { get; }
		IGraphMapMarkerService MapMarkerService { get; }
		IGraphCutsceneService CutsceneService { get; }
		IGraphCheckpointService CheckpointService { get; }
		IGraphQuestService QuestService { get; }
	}
}