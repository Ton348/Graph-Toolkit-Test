using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace GraphCore.Editor
{
	[Serializable]
	public abstract class CommonGraphEditorNode : Node
	{
		public const string ExecutionPortName = "Next";
		public const string TitleOption = "NodeTitle";
		public const string DescriptionOption = "NodeDescription";

		private static readonly BindingFlags s_instancePublicAndNonPublic =
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly BindingFlags s_instancePublic = BindingFlags.Instance | BindingFlags.Public;

		protected virtual string defaultTitle => GetType().Name;
		protected virtual string defaultDescription => string.Empty;

		protected void AddInputExecutionPort(IPortDefinitionContext context)
		{
			context.AddInputPort(ExecutionPortName)
				.WithDisplayName(string.Empty)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}

		protected void AddOutputExecutionPort(IPortDefinitionContext context)
		{
			context.AddOutputPort(ExecutionPortName)
				.WithDisplayName(string.Empty)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			_ = context.AddOption<string>(TitleOption)
				.WithDisplayName("Название")
				.WithDefaultValue(defaultTitle)
				.Build();

			INodeOption descriptionOption = context.AddOption<string>(DescriptionOption)
				.WithDisplayName("Описание")
				.WithDefaultValue(defaultDescription)
				.Build();

			TryEnableMultiline(descriptionOption);
		}

		private static void TryEnableMultiline(INodeOption option)
		{
			if (option == null)
			{
				return;
			}

			object portModel = GetPortModel(option);
			if (portModel == null)
			{
				return;
			}

			List<Attribute> attributes = GetAttributes(portModel);
			if (attributes == null)
			{
				attributes = new List<Attribute>();
			}

			var hasMultilineAttribute = false;
			for (var i = 0; i < attributes.Count; i++)
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
			PropertyInfo portModelProperty = option.GetType().GetProperty("PortModel", s_instancePublicAndNonPublic);
			return portModelProperty?.GetValue(option);
		}

		private static List<Attribute> GetAttributes(object portModel)
		{
			PropertyInfo attributesProperty = portModel.GetType().GetProperty("Attributes", s_instancePublic);
			if (attributesProperty == null)
			{
				return null;
			}

			var attributes = attributesProperty.GetValue(portModel) as IReadOnlyList<Attribute>;
			return attributes == null ? null : new List<Attribute>(attributes);
		}

		private static void SetAttributes(object portModel, List<Attribute> attributes)
		{
			MethodInfo setAttributesMethod =
				portModel.GetType().GetMethod("SetAttributes", s_instancePublicAndNonPublic);
			setAttributesMethod?.Invoke(portModel, new object[] { attributes });
		}
	}
}