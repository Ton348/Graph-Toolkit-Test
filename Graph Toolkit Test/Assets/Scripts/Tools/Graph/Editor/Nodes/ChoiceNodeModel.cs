using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class ChoiceNodeModel : BusinessQuestEditorNode
{
    public const string OPTION1_ID = "Option1Id";
    public const string OPTION1_LABEL = "Option1Label";
    public const string OPTION2_ID = "Option2Id";
    public const string OPTION2_LABEL = "Option2Label";
    public const string OPTION3_ID = "Option3Id";
    public const string OPTION3_LABEL = "Option3Label";
    protected override string DefaultTitle => "Выбор";
    protected override string DefaultDescription => "Показывает игроку варианты выбора.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(OPTION1_ID).WithDisplayName("Option 1 Id");
        context.AddOption<string>(OPTION1_LABEL).WithDisplayName("Option 1 Label");
        context.AddOption<string>(OPTION2_ID).WithDisplayName("Option 2 Id");
        context.AddOption<string>(OPTION2_LABEL).WithDisplayName("Option 2 Label");
        context.AddOption<string>(OPTION3_ID).WithDisplayName("Option 3 Id");
        context.AddOption<string>(OPTION3_LABEL).WithDisplayName("Option 3 Label");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Option1").WithDisplayName("Option 1").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Option2").WithDisplayName("Option 2").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Option3").WithDisplayName("Option 3").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
