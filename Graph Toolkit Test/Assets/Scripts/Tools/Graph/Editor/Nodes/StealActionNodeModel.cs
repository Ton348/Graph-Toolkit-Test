using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class StealActionNodeModel : BusinessQuestEditorNode
{
    public const string STEAL_AMOUNT_OPTION = "StealAmount";
    public const string CAN_FAIL_OPTION = "CanFail";
    public const string REQUIRED_SPEECH_OPTION = "RequiredSpeech";

    protected override string DefaultTitle => "Кража";
    protected override string DefaultDescription => "Пытается украсть деньги у NPC.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<int>(STEAL_AMOUNT_OPTION).WithDisplayName("Сумма").WithDefaultValue(100);
        context.AddOption<bool>(CAN_FAIL_OPTION).WithDisplayName("Может провалиться").WithDefaultValue(true);
        context.AddOption<int>(REQUIRED_SPEECH_OPTION).WithDisplayName("Требуемая речь").WithDefaultValue(0);
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
