using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;
using System.Collections.Generic;
using System.Threading;
using System;

public interface IGraphCheckpointService
{
	UniTask<bool> SaveAsync(string checkpointId, CancellationToken cancellationToken);
	UniTask<bool> ClearAsync(string checkpointId, CancellationToken cancellationToken);
}
