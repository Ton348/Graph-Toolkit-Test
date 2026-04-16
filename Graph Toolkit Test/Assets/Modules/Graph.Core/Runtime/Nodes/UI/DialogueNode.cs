using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.UI
{
	[Serializable]
	public sealed class DialogueNode : CoreGraphNextNode
	{
		public string dialogueTitle;
		public string body;

		public DialogueNode()
		{
			Title = "Dialogue";
			Description = "Shows dialogue and continues";
		}
	}
}