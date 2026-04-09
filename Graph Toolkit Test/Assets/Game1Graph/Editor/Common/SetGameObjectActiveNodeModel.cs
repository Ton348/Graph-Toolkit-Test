using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;

[Serializable]
public class SetGameObjectActiveNodeModel : BusinessQuestCommonNodeModel
{
    public const string TARGET_OBJECT_OPTION = "TargetObject";
    public const string IS_ACTIVE_OPTION = "IsActive";
    public const string VISUAL_ID_OPTION = "VisualId";
    public const string SITE_ID_OPTION = "SiteId";
    public const string LOT_ID_OPTION = "LotId";
    public const string LEGACY_SPAWN_KEY_OPTION = "SpawnKey";

    protected override string DefaultTitle => "Построить/убрать объект";
    protected override string DefaultDescription => "Меняет persistent visual state участка через сервер и siteId.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<GameObject>(TARGET_OBJECT_OPTION)
            .WithDisplayName("Prefab Reference");

        context.AddOption<string>(SITE_ID_OPTION)
            .WithDisplayName("Site Id");

        context.AddOption<string>(VISUAL_ID_OPTION)
            .WithDisplayName("Visual Id");

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
