using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;
using System.Collections.Generic;
using System.Threading;
using System;

public interface IGraphCutsceneService
{
	UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
}
