using System;
using System.Collections.Generic;

namespace GraphCore.Runtime.Nodes.UI
{
	[Serializable]
	public sealed class ChoiceNode : BaseGraphNode
	{
		public List<ChoiceOption> options = new(4)
		{
			new ChoiceOption(),
			new ChoiceOption(),
			new ChoiceOption(),
			new ChoiceOption()
		};

		public ChoiceNode()
		{
			title = "Choice";
			description = "Shows player choices and branches by selection";
		}
	}
}