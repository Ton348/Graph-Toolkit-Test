using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.GraphToolkit.Editor;

[InitializeOnLoad]
internal static class ConditionNodeViewDecorator
{
    const string NodeClassName = "ge-node";
    const string FieldClassName = "ge-model-property-field";

    static double s_NextUpdateTime;

    static ConditionNodeViewDecorator()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    static void OnEditorUpdate()
    {
        if (EditorApplication.timeSinceStartup < s_NextUpdateTime)
        {
            return;
        }

        s_NextUpdateTime = EditorApplication.timeSinceStartup + 0.2d;
        UpdateAllGraphWindows();
    }

    static void UpdateAllGraphWindows()
    {
        var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        foreach (var window in windows)
        {
            if (window == null)
            {
                continue;
            }

            if (window.GetType().FullName != "Unity.GraphToolkit.Editor.GraphViewEditorWindow")
            {
                continue;
            }

            var root = window.rootVisualElement;
            if (root == null)
            {
                continue;
            }

            UpdateGraphRoot(root);
        }
    }

    static void UpdateGraphRoot(VisualElement root)
    {
        var nodes = root.Query<VisualElement>(className: NodeClassName).ToList();
        foreach (var nodeElement in nodes)
        {
            TryDecorateNode(nodeElement);
        }
    }

    static void TryDecorateNode(VisualElement nodeElement)
    {
        var model = GetModel(nodeElement);
        if (model == null)
        {
            return;
        }

        var userNode = GetUserNode(model);
        if (userNode is ConditionNodeModel conditionNode)
        {
            ApplyVisibility(nodeElement, GetConditionType(conditionNode));
            return;
        }

        if (userNode is WaitForConditionNodeModel waitNode)
        {
            ApplyVisibility(nodeElement, GetConditionType(waitNode));
        }
    }

    static ConditionType GetConditionType(Node node)
    {
        var option = node.GetNodeOptionByName(ConditionNodeModel.CONDITION_TYPE_OPTION);
        if (option != null && option.TryGetValue(out ConditionType value))
        {
            return value;
        }

        return ConditionType.BuildingOwned;
    }

    static void ApplyVisibility(VisualElement root, ConditionType conditionType)
    {
        bool showBuilding = conditionType == ConditionType.BuildingOwned;
        bool showMoney = conditionType == ConditionType.HasEnoughMoney;
        bool showStat = conditionType == ConditionType.PlayerStatAtLeast;
        bool showQuest = conditionType == ConditionType.QuestActive || conditionType == ConditionType.QuestCompleted;

        SetOptionVisible(root, ConditionNodeModel.BUILDING_LABEL, showBuilding);
        SetOptionVisible(root, ConditionNodeModel.REQUIRED_MONEY_LABEL, showMoney);
        SetOptionVisible(root, ConditionNodeModel.PLAYER_STAT_LABEL, showStat);
        SetOptionVisible(root, ConditionNodeModel.REQUIRED_STAT_LABEL, showStat);
        SetOptionVisible(root, ConditionNodeModel.QUEST_ID_LABEL, showQuest);
    }

    static void SetOptionVisible(VisualElement root, string labelText, bool visible)
    {
        if (root == null || string.IsNullOrEmpty(labelText))
        {
            return;
        }

        var labels = root.Query<Label>().ToList();
        foreach (var label in labels)
        {
            if (label == null || label.text != labelText)
            {
                continue;
            }

            var field = FindAncestorWithClass(label, FieldClassName);
            if (field == null)
            {
                field = label.parent;
            }

            if (field != null)
            {
                field.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }

    static VisualElement FindAncestorWithClass(VisualElement element, string className)
    {
        var current = element;
        for (int i = 0; i < 8 && current != null; i++)
        {
            if (current.ClassListContains(className))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    static object GetModel(VisualElement nodeElement)
    {
        var property = nodeElement.GetType().GetProperty("Model", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return property?.GetValue(nodeElement);
    }

    static Node GetUserNode(object model)
    {
        var property = model.GetType().GetProperty("Node", BindingFlags.Instance | BindingFlags.Public);
        return property?.GetValue(model) as Node;
    }
}
