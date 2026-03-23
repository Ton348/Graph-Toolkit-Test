using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;

[Serializable]
public class SetGameObjectActiveNodeModel : BusinessQuestEditorNode
{
    public const string TARGET_OBJECT_OPTION = "TargetObject";
    public const string IS_ACTIVE_OPTION = "IsActive";

    protected override string DefaultTitle => "Включить/выключить объект";
    protected override string DefaultDescription => "Включает или выключает указанный объект";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<GameObject>(TARGET_OBJECT_OPTION)
            .WithDisplayName("Объект");

        context.AddOption<bool>(IS_ACTIVE_OPTION)
            .WithDisplayName("Включить")
            .WithDefaultValue(true);
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
