using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Cinematics
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
