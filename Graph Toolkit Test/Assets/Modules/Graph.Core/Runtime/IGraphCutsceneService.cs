using System.Threading;
using Cysharp.Threading.Tasks;

namespace GraphCore.Runtime
{
	public interface IGraphCutsceneService
	{
		UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
	}
}