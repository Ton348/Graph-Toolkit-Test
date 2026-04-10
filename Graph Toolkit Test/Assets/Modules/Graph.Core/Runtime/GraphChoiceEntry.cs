using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;

public readonly struct GraphChoiceEntry
{
	public readonly string label;

	public GraphChoiceEntry(string label)
	{
		this.label = label;
	}
}
