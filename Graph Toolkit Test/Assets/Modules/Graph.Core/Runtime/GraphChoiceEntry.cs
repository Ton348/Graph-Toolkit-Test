using System;

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
