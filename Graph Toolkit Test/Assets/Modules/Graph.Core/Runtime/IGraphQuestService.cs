using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphQuestService
	{
		UniTask<bool> StartQuestAsync(string questId, CancellationToken cancellationToken);
		UniTask<bool> CompleteQuestAsync(string questId, CancellationToken cancellationToken);
		UniTask<QuestState> GetQuestStateAsync(string questId, CancellationToken cancellationToken);
	}
}
