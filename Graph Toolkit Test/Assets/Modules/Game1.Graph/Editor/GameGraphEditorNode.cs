using System;
using Game1.Graph.Editor.Infrastructure;
using Game1.Graph.Runtime.Infrastructure;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace Game1.Graph.Editor
{
	[Serializable]
	public abstract class GameGraphEditorNode : CommonGraphEditorNode
	{
		protected virtual string category => GameGraphNodeCategories.Common;

		protected string BuildCategoryPath(string nodeName)
		{
			if (string.IsNullOrWhiteSpace(nodeName))
			{
				return category;
			}

			return $"{category}/{nodeName}";
		}

		protected void AddNextPort(IPortDefinitionContext context)
		{
			context.AddOutputPort(GameGraphPortNames.Next)
				.WithDisplayName(GameGraphPortNames.Next)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}

		protected void AddSuccessFailPorts(IPortDefinitionContext context)
		{
			context.AddOutputPort(GameGraphPortNames.Success)
				.WithDisplayName(GameGraphPortNames.Success)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(GameGraphPortNames.Fail)
				.WithDisplayName(GameGraphPortNames.Fail)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}

		protected void AddTrueFalsePorts(IPortDefinitionContext context)
		{
			context.AddOutputPort(GameGraphPortNames.True)
				.WithDisplayName(GameGraphPortNames.True)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(GameGraphPortNames.False)
				.WithDisplayName(GameGraphPortNames.False)
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}

		protected void AddStringOption(
			IOptionDefinitionContext context,
			string optionKey,
			string displayName,
			string defaultValue = "")
		{
			context.AddOption<string>(optionKey)
				.WithDisplayName(displayName)
				.WithDefaultValue(defaultValue);
		}

		protected void AddIntOption(
			IOptionDefinitionContext context,
			string optionKey,
			string displayName,
			int defaultValue = 0)
		{
			context.AddOption<int>(optionKey)
				.WithDisplayName(displayName)
				.WithDefaultValue(defaultValue);
		}

		protected void AddFloatOption(
			IOptionDefinitionContext context,
			string optionKey,
			string displayName,
			float defaultValue = 0f)
		{
			context.AddOption<float>(optionKey)
				.WithDisplayName(displayName)
				.WithDefaultValue(defaultValue);
		}

		protected void AddBoolOption(
			IOptionDefinitionContext context,
			string optionKey,
			string displayName,
			bool defaultValue = false)
		{
			context.AddOption<bool>(optionKey)
				.WithDisplayName(displayName)
				.WithDefaultValue(defaultValue);
		}
	}
}