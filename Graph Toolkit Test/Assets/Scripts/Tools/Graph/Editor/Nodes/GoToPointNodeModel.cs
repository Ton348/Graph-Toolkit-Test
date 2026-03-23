using System;

[Serializable]
public class GoToPointNodeModel : BusinessQuestEditorNode
{
    public const string MARKER_ID_OPTION = "MarkerId";
    public const string ARRIVAL_OPTION = "ArrivalDistance";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<string>(MARKER_ID_OPTION).WithDisplayName("Marker Id");
        context.AddOption<float>(ARRIVAL_OPTION).WithDisplayName("Arrival Distance").WithDefaultValue(2f);
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
