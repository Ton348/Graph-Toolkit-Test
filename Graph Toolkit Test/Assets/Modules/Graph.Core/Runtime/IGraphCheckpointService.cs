using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace GraphCore.Runtime
{
	public interface IGraphCheckpointService
	{
		UniTask<bool> SaveAsync(string checkpointId, CancellationToken cancellationToken);
		UniTask<bool> ClearAsync(string checkpointId, CancellationToken cancellationToken);
	}
}
