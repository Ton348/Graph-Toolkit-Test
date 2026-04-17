using System.Threading;
using Cysharp.Threading.Tasks;

namespace Graph.Core.Runtime
{
	public interface IGraphCutsceneService
	{
		UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
	}
}