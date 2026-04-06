namespace Graph.Features.Quest.Runtime
{
    public class CheckpointNode : global::CheckpointNode { }
    public class RefreshProfileNode : global::RefreshProfileNode { }
    public class RequestStartQuestNode : global::RequestStartQuestNode { }
    public class RequestCompleteQuestNode : global::RequestCompleteQuestNode { }

    public enum QuestActionType
    {
        None = global::QuestActionType.None,
        StartQuest = global::QuestActionType.StartQuest,
        CompleteQuest = global::QuestActionType.CompleteQuest
    }
}
