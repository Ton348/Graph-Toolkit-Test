using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphChoiceService
	{
		UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken);
	}
}
