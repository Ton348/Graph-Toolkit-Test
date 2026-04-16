using System.Threading;
using Cysharp.Threading.Tasks;

namespace GraphCore.Runtime
{
	public interface IGraphDialogueService
	{
		UniTask ShowAsync(string title, string body, CancellationToken cancellationToken);
		void EndConversation();
	}
}