using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace GraphCore.Runtime
{
	public interface IGraphChoiceService
	{
		UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken);
	}
}
