using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Unity.GraphToolkit.Editor;

[Serializable]
public abstract class BusinessQuestEditorNode : Node
{
    public const string EXECUTION_PORT_NAME = "Next";
    public const string TITLE_OPTION = "NodeTitle";
    public const string DESCRIPTION_OPTION = "NodeDescription";
    public const string COMMENT_OPTION = "NodeComment";

    protected virtual string DefaultTitle => GetType().Name;
    protected virtual string DefaultDescription => string.Empty;

    protected void AddInputExecutionPort(IPortDefinitionContext context)
    {
        context.AddInputPort(EXECUTION_PORT_NAME)
            .WithDisplayName(string.Empty)
            .WithConnectorUI(PortConnectorUI.Arrowhead)
            .Build();
    }

    protected void AddOutputExecutionPort(IPortDefinitionContext context)
    {
        context.AddOutputPort(EXECUTION_PORT_NAME)
            .WithDisplayName(string.Empty)
            .WithConnectorUI(PortConnectorUI.Arrowhead)
            .Build();
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        var titleOption = context.AddOption<string>(TITLE_OPTION)
            .WithDisplayName("Название")
            .WithDefaultValue(DefaultTitle)
            .Build();

        var descriptionOption = context.AddOption<string>(DESCRIPTION_OPTION)
            .WithDisplayName("Описание")
            .WithDefaultValue(DefaultDescription)
            .Build();

        var commentOption = context.AddOption<string>(COMMENT_OPTION)
            .WithDisplayName("Комментарий")
            .WithDefaultValue(string.Empty)
            .Build();

        TryEnableMultiline(descriptionOption);
        TryEnableMultiline(commentOption);
    }

    static void TryEnableMultiline(INodeOption option)
    {
        if (option == null)
        {
            return;
        }

        var portModel = GetPortModel(option);
        if (portModel == null)
        {
            return;
        }

        var attributes = GetAttributes(portModel);
        if (attributes == null)
        {
            attributes = new List<Attribute>();
        }

        if (!attributes.Any(a => a is MultilineAttribute))
        {
            attributes.Add(new MultilineAttribute());
        }

        SetAttributes(portModel, attributes);
    }

    static object GetPortModel(INodeOption option)
    {
        var portModelProperty = option.GetType().GetProperty("PortModel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return portModelProperty?.GetValue(option);
    }

    static List<Attribute> GetAttributes(object portModel)
    {
        var attributesProperty = portModel.GetType().GetProperty("Attributes", BindingFlags.Instance | BindingFlags.Public);
        if (attributesProperty == null)
        {
            return null;
        }

        var attributes = attributesProperty.GetValue(portModel) as IReadOnlyList<Attribute>;
        return attributes == null ? null : new List<Attribute>(attributes);
    }

    static void SetAttributes(object portModel, List<Attribute> attributes)
    {
        var setAttributesMethod = portModel.GetType().GetMethod("SetAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
        setAttributesMethod?.Invoke(portModel, new object[] { attributes });
    }
}
