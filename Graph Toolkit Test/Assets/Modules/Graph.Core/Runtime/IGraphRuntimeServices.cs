using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphRuntimeServices
	{
		IGraphDialogueService dialogueService { get; }
		IGraphChoiceService choiceService { get; }
		IGraphMapMarkerService mapMarkerService { get; }
		IGraphCutsceneService cutsceneService { get; }
		IGraphCheckpointService checkpointService { get; }
		IGraphQuestService questService { get; }
	}
}
