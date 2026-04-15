using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;
using System.Collections.Generic;
using System.Threading;
using System;

public interface IGraphDialogueService
{
	UniTask ShowAsync(string title, string body, CancellationToken cancellationToken);
	void EndConversation();
}
