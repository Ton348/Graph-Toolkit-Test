using System;

[Serializable]
public class WaitForBuildingUpgradedNodeModel : BusinessQuestEditorNode
{
    public const string BUILDING_ID_OPTION = "BuildingId";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<string>(BUILDING_ID_OPTION)
            .WithDisplayName("Building Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
