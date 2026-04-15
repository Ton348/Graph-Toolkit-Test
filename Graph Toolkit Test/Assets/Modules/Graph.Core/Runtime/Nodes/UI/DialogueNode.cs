using System;

namespace GraphCore.BaseNodes.Runtime.UI
{
	[Serializable]
	public sealed class DialogueNode : BaseGraphNode
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
