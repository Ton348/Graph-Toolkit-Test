using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public interface IGraphChoiceService
{
	UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken);
}
