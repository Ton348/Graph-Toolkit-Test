using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class SkillCheckNodeModel : BusinessQuestEditorNode
{
    public const string SKILL_OPTION = "Skill";
    public const string REQUIRED_OPTION = "Required";
    protected override string DefaultTitle => "Проверка навыка";
    protected override string DefaultDescription => "Проверяет навык и ведет по успеху/провалу.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<SkillType>(SKILL_OPTION).WithDisplayName("Skill");
        context.AddOption<int>(REQUIRED_OPTION).WithDisplayName("Required Value");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
