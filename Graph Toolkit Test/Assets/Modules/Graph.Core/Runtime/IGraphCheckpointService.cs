using System.Threading;
using Cysharp.Threading.Tasks;

namespace Graph.Core.Runtime
{
	public interface IGraphCheckpointService
	{
		UniTask<bool> SaveAsync(string graphId, string checkpointId, CancellationToken cancellationToken);
		UniTask<bool> ClearAsync(string graphId, string checkpointId, CancellationToken cancellationToken);
	}
}
