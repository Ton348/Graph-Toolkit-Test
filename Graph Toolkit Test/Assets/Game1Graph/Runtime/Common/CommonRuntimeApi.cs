namespace Graph.Features.Common.Runtime
{
    public class StartNode : global::StartNode { }
    public class EndNode : global::EndNode { }
    public class DialogueNode : global::DialogueNode { }
    public class ChoiceNode : global::ChoiceNode { }
    public class ConditionNode : global::ConditionNode { }
    public class GoToPointNode : global::GoToPointNode { }
    public class SetGameObjectActiveNode : global::SetGameObjectActiveNode { }
    public class AddMapMarkerNode : global::AddMapMarkerNode { }

    public enum ConditionType
    {
        BuildingOwned = global::ConditionType.BuildingOwned,
        HasEnoughMoney = global::ConditionType.HasEnoughMoney,
        PlayerStatAtLeast = global::ConditionType.PlayerStatAtLeast,
        QuestActive = global::ConditionType.QuestActive,
        QuestCompleted = global::ConditionType.QuestCompleted
    }

    public enum PlayerStatType
    {
        Bargaining = global::PlayerStatType.Bargaining,
        Speech = global::PlayerStatType.Speech,
        Speed = global::PlayerStatType.Speed,
        Damage = global::PlayerStatType.Damage,
        Health = global::PlayerStatType.Health
    }

    public enum SkillType
    {
        Bargaining = global::SkillType.Bargaining,
        Speech = global::SkillType.Speech,
        Speed = global::SkillType.Speed,
        Damage = global::SkillType.Damage,
        Health = global::SkillType.Health
    }

    public class ChoiceOption : global::ChoiceOption { }
}
