using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GraphCore.Runtime
{
	public interface IGraphChoiceService
	{
		UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken);
	}
}