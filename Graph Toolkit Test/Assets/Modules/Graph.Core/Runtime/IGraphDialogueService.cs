using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public interface IGraphDialogueService
{
	UniTask ShowAsync(string title, string body, CancellationToken cancellationToken);
	void EndConversation();
}
