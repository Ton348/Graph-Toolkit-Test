using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class ChoiceNodeModel : BusinessQuestCommonNodeModel
{
    public const string OPTION1_LABEL = "Option1Label";
    public const string OPTION2_LABEL = "Option2Label";
    public const string OPTION3_LABEL = "Option3Label";
    public const string OPTION4_LABEL = "Option4Label";
    protected override string DefaultTitle => "Выбор";
    protected override string DefaultDescription => "Показывает игроку варианты выбора.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(OPTION1_LABEL).WithDisplayName("Option 1 Label");
        context.AddOption<string>(OPTION2_LABEL).WithDisplayName("Option 2 Label");
        context.AddOption<string>(OPTION3_LABEL).WithDisplayName("Option 3 Label");
        context.AddOption<string>(OPTION4_LABEL).WithDisplayName("Option 4 Label");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Option1").WithDisplayName("Option 1").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Option2").WithDisplayName("Option 2").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Option3").WithDisplayName("Option 3").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Option4").WithDisplayName("Option 4").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
