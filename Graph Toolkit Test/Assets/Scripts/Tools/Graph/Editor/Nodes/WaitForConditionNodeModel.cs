using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class WaitForConditionNodeModel : BusinessQuestEditorNode
{
    public const string CONDITION_TYPE_OPTION = "ConditionType";
    public const string BUILDING_OPTION = "TargetBuilding";
    public const string REQUIRED_MONEY_OPTION = "RequiredMoney";
    public const string PLAYER_STAT_OPTION = "PlayerStat";
    public const string REQUIRED_STAT_OPTION = "RequiredStatValue";

    public const string CONDITION_TYPE_LABEL = "Условие";
    public const string BUILDING_LABEL = "Здание";
    public const string REQUIRED_MONEY_LABEL = "Требуемые деньги";
    public const string PLAYER_STAT_LABEL = "Стат";
    public const string REQUIRED_STAT_LABEL = "Значение стата";

    protected override string DefaultTitle => "Ожидание условия";
    protected override string DefaultDescription => "Ожидает, пока условие станет истинным.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<ConditionType>(CONDITION_TYPE_OPTION).WithDisplayName(CONDITION_TYPE_LABEL);
        context.AddOption<BuildingDefinition>(BUILDING_OPTION).WithDisplayName(BUILDING_LABEL);
        context.AddOption<int>(REQUIRED_MONEY_OPTION).WithDisplayName(REQUIRED_MONEY_LABEL);
        context.AddOption<PlayerStatType>(PLAYER_STAT_OPTION).WithDisplayName(PLAYER_STAT_LABEL);
        context.AddOption<int>(REQUIRED_STAT_OPTION).WithDisplayName(REQUIRED_STAT_LABEL);
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
