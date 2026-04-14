using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Unity.GraphToolkit.Editor;

[Serializable]
public abstract class CommonGraphEditorNode : Node
{
	private static readonly BindingFlags InstancePublicAndNonPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	private static readonly BindingFlags InstancePublic = BindingFlags.Instance | BindingFlags.Public;
	public const string EXECUTION_PORT_NAME = "Next";
	public const string TITLE_OPTION = "NodeTitle";
	public const string DESCRIPTION_OPTION = "NodeDescription";

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

		TryEnableMultiline(descriptionOption);
	}

	private static void TryEnableMultiline(INodeOption option)
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

		bool hasMultilineAttribute = false;
		for (int i = 0; i < attributes.Count; i++)
		{
			if (attributes[i] is MultilineAttribute)
			{
				hasMultilineAttribute = true;
				break;
			}
		}

		if (!hasMultilineAttribute)
		{
			attributes.Add(new MultilineAttribute());
		}

		SetAttributes(portModel, attributes);
	}

	private static object GetPortModel(INodeOption option)
	{
		PropertyInfo portModelProperty = option.GetType().GetProperty("PortModel", InstancePublicAndNonPublic);
		return portModelProperty?.GetValue(option);
	}

	private static List<Attribute> GetAttributes(object portModel)
	{
		PropertyInfo attributesProperty = portModel.GetType().GetProperty("Attributes", InstancePublic);
		if (attributesProperty == null)
		{
			return null;
		}

		IReadOnlyList<Attribute> attributes = attributesProperty.GetValue(portModel) as IReadOnlyList<Attribute>;
		return attributes == null ? null : new List<Attribute>(attributes);
	}

	private static void SetAttributes(object portModel, List<Attribute> attributes)
	{
		MethodInfo setAttributesMethod = portModel.GetType().GetMethod("SetAttributes", InstancePublicAndNonPublic);
		setAttributesMethod?.Invoke(portModel, new object[] { attributes });
	}
}
