using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;
using System.Collections.Generic;
using System.Threading;
using System;

public interface IGraphChoiceService
{
	UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken);
}
