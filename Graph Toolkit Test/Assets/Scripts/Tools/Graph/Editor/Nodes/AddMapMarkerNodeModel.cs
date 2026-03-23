using System;

[Serializable]
public class AddMapMarkerNodeModel : BusinessQuestEditorNode
{
    public const string MARKER_ID_OPTION = "MarkerId";
    public const string TITLE_OPTION = "Title";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<string>(MARKER_ID_OPTION).WithDisplayName("Marker Id");
        context.AddOption<string>(TITLE_OPTION).WithDisplayName("Title");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
