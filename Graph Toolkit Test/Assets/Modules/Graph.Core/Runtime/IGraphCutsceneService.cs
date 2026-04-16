using Cysharp.Threading.Tasks;
using System.Threading;

namespace GraphCore.Runtime
{
	public interface IGraphCutsceneService
	{
		UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
	}
}
