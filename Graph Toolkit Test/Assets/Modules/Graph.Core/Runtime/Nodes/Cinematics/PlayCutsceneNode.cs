using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Cinematics
{
	[Serializable]
	public sealed class PlayCutsceneNode : CoreGraphNextNode
	{
		public string cutsceneReference;

		public PlayCutsceneNode()
		{
			Title = "Play Cutscene";
			Description = "Plays a cutscene and continues";
		}
	}
}