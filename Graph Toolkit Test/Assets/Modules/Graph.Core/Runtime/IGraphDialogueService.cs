using Cysharp.Threading.Tasks;
using System.Threading;

namespace GraphCore.Runtime
{
	public interface IGraphDialogueService
	{
		UniTask ShowAsync(string title, string body, CancellationToken cancellationToken);
		void EndConversation();
	}
}
