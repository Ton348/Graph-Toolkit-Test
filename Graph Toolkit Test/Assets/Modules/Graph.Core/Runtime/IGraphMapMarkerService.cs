using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphMapMarkerService
	{
		void ShowOrUpdateMarker(string markerId, string targetObjectName);
	}
}
