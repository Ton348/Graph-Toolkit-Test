using System;

namespace Graph.Features.Common.Editor
{
    [Serializable] public abstract class StartNodeModel : global::StartNodeModel { }
    [Serializable] public abstract class EndNodeModel : global::EndNodeModel { }
    [Serializable] public abstract class DialogueNodeModel : global::DialogueNodeModel { }
    [Serializable] public abstract class ChoiceNodeModel : global::ChoiceNodeModel { }
    [Serializable] public abstract class ConditionNodeModel : global::ConditionNodeModel { }
    [Serializable] public abstract class GoToPointNodeModel : global::GoToPointNodeModel { }
    [Serializable] public abstract class SetGameObjectActiveNodeModel : global::SetGameObjectActiveNodeModel { }
    [Serializable] public abstract class AddMapMarkerNodeModel : global::AddMapMarkerNodeModel { }
}
