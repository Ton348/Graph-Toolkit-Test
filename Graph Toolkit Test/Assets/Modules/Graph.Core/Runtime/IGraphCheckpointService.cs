using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphCheckpointService
	{
		UniTask<bool> SaveAsync(string checkpointId, CancellationToken cancellationToken);
		UniTask<bool> ClearAsync(string checkpointId, CancellationToken cancellationToken);
	}
}
