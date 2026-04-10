using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public interface IGraphRuntimeServices
{
	IGraphDialogueService dialogueService { get; }
	IGraphChoiceService choiceService { get; }
	IGraphMapMarkerService mapMarkerService { get; }
	IGraphCutsceneService cutsceneService { get; }
	IGraphCheckpointService checkpointService { get; }
	IGraphQuestService questService { get; }
}
