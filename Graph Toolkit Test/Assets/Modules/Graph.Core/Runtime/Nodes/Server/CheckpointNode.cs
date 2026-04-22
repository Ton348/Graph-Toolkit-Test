using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class CheckpointNode : CoreGraphSuccessFailNode
	{
		public string checkpointId;
		public CheckpointAction action;

		public CheckpointNode()
		{
			Title = "Управление чекпоинтом";
			Description = "Сохраняет или удаляет checkpoint с которого начнется старт графа в профиле игрока";
		}
	}
}
