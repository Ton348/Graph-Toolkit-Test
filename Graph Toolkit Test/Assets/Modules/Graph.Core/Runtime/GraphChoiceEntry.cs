using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Collections.Generic;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public readonly struct GraphChoiceEntry
	{
		public readonly string label;

		public GraphChoiceEntry(string label)
		{
			this.label = label;
		}
	}
}
