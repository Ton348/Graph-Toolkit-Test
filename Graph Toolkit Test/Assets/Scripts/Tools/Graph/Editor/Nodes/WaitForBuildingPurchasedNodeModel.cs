using System;

[Serializable]
public class WaitForBuildingPurchasedNodeModel : BusinessQuestEditorNode
{
    public const string BUILDING_ID_OPTION = "BuildingId";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(BUILDING_ID_OPTION)
            .WithDisplayName("Building Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }

    protected override string DefaultTitle => "Ожидание покупки здания";
    protected override string DefaultDescription => "Ожидает, пока игрок купит указанное здание.";
}
