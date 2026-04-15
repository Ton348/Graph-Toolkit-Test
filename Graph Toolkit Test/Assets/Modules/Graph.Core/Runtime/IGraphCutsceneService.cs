using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphCutsceneService
	{
		UniTask PlayAsync(string cutsceneReference, CancellationToken cancellationToken);
	}
}
