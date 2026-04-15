using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;
using System.Collections.Generic;
using System.Threading;
using System;

public interface IGraphQuestService
{
	UniTask<bool> StartQuestAsync(string questId, CancellationToken cancellationToken);
	UniTask<bool> CompleteQuestAsync(string questId, CancellationToken cancellationToken);
	UniTask<QuestState> GetQuestStateAsync(string questId, CancellationToken cancellationToken);
}
