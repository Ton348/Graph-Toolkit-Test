using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Nodes.Server;

namespace Graph.Core.Runtime
{
	public interface IGraphQuestService
	{
		UniTask<bool> StartQuestAsync(string questId, CancellationToken cancellationToken);
		UniTask<bool> CompleteQuestAsync(string questId, CancellationToken cancellationToken);
		UniTask<QuestState> GetQuestStateAsync(string questId, CancellationToken cancellationToken);
	}
}