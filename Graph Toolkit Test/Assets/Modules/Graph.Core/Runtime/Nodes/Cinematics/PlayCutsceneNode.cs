using System;

namespace GraphCore.Runtime.Nodes.Cinematics
{
	[Serializable]
	public sealed class PlayCutsceneNode : BaseGraphNode
	{
		public string cutsceneReference;

		public PlayCutsceneNode()
		{
			Title = "Play Cutscene";
			Description = "Plays a cutscene and continues";
		}
	}
}
