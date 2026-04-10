using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public sealed class GraphRuntimeServices : IGraphRuntimeServices
{
	public IGraphDialogueService dialogueService { get; }
	public IGraphChoiceService choiceService { get; }
	public IGraphMapMarkerService mapMarkerService { get; }
	public IGraphCutsceneService cutsceneService { get; }
	public IGraphCheckpointService checkpointService { get; }
	public IGraphQuestService questService { get; }

	public GraphRuntimeServices(
		IGraphDialogueService dialogueService,
		IGraphChoiceService choiceService,
		IGraphMapMarkerService mapMarkerService,
		IGraphCutsceneService cutsceneService,
		IGraphCheckpointService checkpointService,
		IGraphQuestService questService)
	{
		this.dialogueService = dialogueService;
		this.choiceService = choiceService;
		this.mapMarkerService = mapMarkerService;
		this.cutsceneService = cutsceneService;
		this.checkpointService = checkpointService;
		this.questService = questService;
	}
}
