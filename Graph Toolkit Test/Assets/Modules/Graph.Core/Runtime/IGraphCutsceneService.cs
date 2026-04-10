using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public interface IGraphCutsceneService
{
	UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
}
