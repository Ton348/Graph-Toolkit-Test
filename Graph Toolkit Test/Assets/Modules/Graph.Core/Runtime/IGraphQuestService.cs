using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;

namespace GraphCore.Runtime
{
	public interface IGraphQuestService
	{
		UniTask<bool> StartQuestAsync(string questId, CancellationToken cancellationToken);
		UniTask<bool> CompleteQuestAsync(string questId, CancellationToken cancellationToken);
		UniTask<QuestState> GetQuestStateAsync(string questId, CancellationToken cancellationToken);
	}
}