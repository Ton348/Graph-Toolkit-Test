using System.Threading;
using Cysharp.Threading.Tasks;

namespace Graph.Core.Runtime
{
	public interface IGraphDialogueService
	{
		UniTask ShowAsync(string title, string body, CancellationToken cancellationToken);
		void EndConversation();
	}
}