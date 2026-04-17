using System.Threading;
using Cysharp.Threading.Tasks;

namespace Graph.Core.Runtime
{
	public interface IGraphCheckpointService
	{
		UniTask<bool> SaveAsync(string checkpointId, CancellationToken cancellationToken);
		UniTask<bool> ClearAsync(string checkpointId, CancellationToken cancellationToken);
	}
}