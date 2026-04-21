using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ContactSelectViewTools
{
    [MenuItem("Tools/UI/Create Contact Select View")]
    public static void CreateContactSelectView()
    {
        GameObject root = new("ContactSelectView", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(520f, 36f);

        HorizontalLayoutGroup rootLayout = root.GetComponent<HorizontalLayoutGroup>();
        rootLayout.padding = new RectOffset(8, 8, 4, 4);
        rootLayout.spacing = 8f;
        rootLayout.childAlignment = TextAnchor.MiddleLeft;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = false;
        rootLayout.childForceExpandHeight = false;

        GameObject valueTextGo = CreateText("SelectedNameText", root.transform, "Нет");
        LayoutElement valueLayout = valueTextGo.AddComponent<LayoutElement>();
        valueLayout.minWidth = 180f;

        GameObject changeButtonGo = CreateButton("ChangeButton", root.transform, "Сменить");
        Button changeButton = changeButtonGo.GetComponent<Button>();

        GameObject popupRoot = new("PopupRoot", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        popupRoot.transform.SetParent(root.transform, false);
        popupRoot.SetActive(false);

        RectTransform popupRect = popupRoot.GetComponent<RectTransform>();
        popupRect.sizeDelta = new Vector2(260f, 100f);

        Image popupImage = popupRoot.GetComponent<Image>();
        popupImage.color = new Color(0.15f, 0.15f, 0.15f, 0.98f);

        VerticalLayoutGroup popupLayout = popupRoot.GetComponent<VerticalLayoutGroup>();
        popupLayout.padding = new RectOffset(8, 8, 8, 8);
        popupLayout.spacing = 6f;
        popupLayout.childControlWidth = true;
        popupLayout.childControlHeight = false;
        popupLayout.childForceExpandWidth = true;
        popupLayout.childForceExpandHeight = false;

        var layoutElement = popupRoot.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        var popupFitter = popupRoot.AddComponent<ContentSizeFitter>();
        popupFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        popupFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject content = new("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(popupRoot.transform, false);

        var contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 6f;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        var contentFitter = content.GetComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject itemPrefab = CreateButton("ItemButtonPrefab", content.transform, "Вариант");
        itemPrefab.SetActive(false);

        Type contactSelectViewType = FindTypeByName("ContactSelectView");
        if (contactSelectViewType == null)
        {
            Debug.LogError("ContactSelectView type was not found. Make sure the runtime script exists and compiles.");
            Undo.RegisterCreatedObjectUndo(root, "Create Contact Select View");
            Selection.activeGameObject = root;
            return;
        }

        Component view = root.AddComponent(contactSelectViewType);
        Assign(view, "selectedNameText", valueTextGo.GetComponent<TMP_Text>());
        Assign(view, "changeButton", changeButton);
        Assign(view, "popupRoot", popupRoot);
        Assign(view, "itemsRoot", content.transform);
        Assign(view, "itemButtonPrefab", itemPrefab.GetComponent<Button>());

        if (Selection.activeTransform != null)
        {
            root.transform.SetParent(Selection.activeTransform, false);
        }

        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Create Contact Select View");
    }

    private static GameObject CreateText(string name, Transform parent, string textValue)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TMP_Text text = go.GetComponent<TMP_Text>();
        text.text = textValue;
        text.fontSize = 24;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string label)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 32f;
        layout.preferredHeight = 32f;

        GameObject labelGo = CreateText("Label", go.transform, label);
        TMP_Text labelText = labelGo.GetComponent<TMP_Text>();
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.black;
        labelText.fontSize = 22;

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return go;
    }

    private static Type FindTypeByName(string typeName)
    {
        for (int i = 0; i < AppDomain.CurrentDomain.GetAssemblies().Length; i++)
        {
            System.Reflection.Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()[i];
            Type type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException)
            {
                continue;
            }

            for (int j = 0; j < types.Length; j++)
            {
                if (types[j] != null && types[j].Name == typeName)
                {
                    return types[j];
                }
            }
        }

        return null;
    }

    private static void Assign(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
    {
        SerializedObject so = new(target);
        SerializedProperty property = so.FindProperty(fieldName);
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}