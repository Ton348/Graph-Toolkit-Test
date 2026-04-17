using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class BusinessManagementWindowBuilder
{
    private const string MenuPath = "Tools/Business/Build Management Window UI";
    private const string WindowName = "BusinessManagementWindow";

    [MenuItem(MenuPath)]
    public static void BuildManagementWindowUi()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();

        GameObject window = FindOrCreateChild(canvas.transform, WindowName);
        ConfigureWindow(window);

        GameObject header = FindOrCreateChild(window.transform, "Header");
        ConfigureHeader(header);

        GameObject topBar = FindOrCreateChild(window.transform, "TopBar");
        ConfigureTopBar(topBar);

        GameObject tabs = FindOrCreateChild(window.transform, "Tabs");
        ConfigureTabs(tabs);

        GameObject tabContentRoot = FindOrCreateChild(window.transform, "TabContentRoot");
        ConfigureTabContentRoot(tabContentRoot);

        GameObject overviewTab = FindOrCreateChild(tabContentRoot.transform, "OverviewTab");
        ConfigureTab(overviewTab);
        BuildOverviewTab(overviewTab.transform);

        GameObject setupTab = FindOrCreateChild(tabContentRoot.transform, "SetupTab");
        ConfigureTab(setupTab);
        BuildSetupTab(setupTab.transform);

        GameObject staffTab = FindOrCreateChild(tabContentRoot.transform, "StaffTab");
        ConfigureTab(staffTab);
        BuildStaffTab(staffTab.transform);

        Selection.activeGameObject = window;
        EditorUtility.SetDirty(window);
        EditorUtility.SetDirty(canvas.gameObject);
        EditorSceneManager.MarkSceneDirty(window.scene);
        Debug.Log("[BusinessManagementWindowBuilder] UI created/updated.");
    }

    private static void ConfigureWindow(GameObject window)
    {
        RectTransform rect = EnsureRectTransform(window);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 620f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(window);
        image.color = new Color(0.12f, 0.14f, 0.19f, 0.92f);

        VerticalLayoutGroup layout = EnsureComponent<VerticalLayoutGroup>(window);
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(window);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private static void ConfigureHeader(GameObject header)
    {
        ConfigureRowContainer(header, 56f, 12f);
        CreateText(header.transform, "TitleText", "Управление бизнесом", 30, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, true, 1f);
        CreateButton(header.transform, "CloseButton", "X", 54f, 44f, new Color(0.7f, 0.7f, 0.7f, 1f));
    }

    private static void ConfigureTopBar(GameObject topBar)
    {
        ConfigureRowContainer(topBar, 56f, 12f);
        CreateText(topBar.transform, "BusinessLabel", "Ваш бизнес:", 24, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, false, 0f, 180f);
        CreateDropdown(topBar.transform, "BusinessDropdown", 320f, 44f);
        CreateButton(topBar.transform, "OpenCloseButton", "Открыть", 180f, 44f, new Color(0.17f, 0.8f, 0.25f, 1f));
    }

    private static void ConfigureTabs(GameObject tabs)
    {
        ConfigureRowContainer(tabs, 48f, 12f);
        CreateButton(tabs.transform, "OverviewTabButton", "Общее", 160f, 40f, new Color(0.85f, 0.85f, 0.85f, 1f), Color.black);
        CreateButton(tabs.transform, "SetupTabButton", "Обустройство", 160f, 40f, new Color(0.85f, 0.85f, 0.85f, 1f), Color.black);
        CreateButton(tabs.transform, "StaffTabButton", "Персонал", 160f, 40f, new Color(0.85f, 0.85f, 0.85f, 1f), Color.black);
    }

    private static void ConfigureTabContentRoot(GameObject root)
    {
        RectTransform rect = EnsureRectTransform(root);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = Vector2.zero;

        LayoutElement layout = EnsureComponent<LayoutElement>(root);
        layout.flexibleHeight = 1f;
        layout.minHeight = 420f;
    }

    private static void ConfigureTab(GameObject tab)
    {
        RectTransform rect = EnsureRectTransform(tab);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = EnsureComponent<Image>(tab);
        image.color = new Color(1f, 1f, 1f, 0.03f);

        VerticalLayoutGroup layout = EnsureComponent<VerticalLayoutGroup>(tab);
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static void BuildOverviewTab(Transform parent)
    {
        CreateValueRow(parent, "IncomeRow", "Доходы", "IncomeValueText");
        CreateValueRow(parent, "ExpensesRow", "Расходы", "ExpensesValueText");
        CreateValueRow(parent, "ProfitRow", "Накопленная прибыль", "ProfitValueText");

        GameObject priceRow = FindOrCreateChild(parent, "PriceRow");
        ConfigureCard(priceRow, 96f);

        GameObject priceLabel = FindOrCreateChild(priceRow.transform, "PriceLabel");
        ConfigureTextObject(priceLabel, "Цена товара", 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);

        GameObject controls = FindOrCreateChild(priceRow.transform, "PriceControls");
        ConfigureRowContainer(controls, 42f, 12f);

        CreateSlider(controls.transform, "PriceSlider", 540f, 24f);
        CreateText(controls.transform, "PriceValueText", "100", 22, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, false, 0f, 120f);
    }

    private static void BuildSetupTab(Transform parent)
    {
        CreateDropdownRow(parent, "StorageRow", "Склад", "StorageDropdown");
        CreateDropdownRow(parent, "CashDeskRow", "Кассы", "CashDeskDropdown");
        CreateDropdownRow(parent, "ShelfRow", "Полки", "ShelfDropdown");
    }

    private static void BuildStaffTab(Transform parent)
    {
        CreateDropdownRow(parent, "SupplierRow", "Поставщик", "SupplierDropdown");
        CreateDropdownRow(parent, "CashierRow", "Кассир", "CashierDropdown");
        CreateDropdownRow(parent, "MerchandiserRow", "Мерчендайзер", "MerchandiserDropdown");
    }

    private static void CreateValueRow(Transform parent, string rowName, string labelText, string valueName)
    {
        GameObject row = FindOrCreateChild(parent, rowName);
        ConfigureRowCard(row, 44f);
        CreateText(row.transform, rowName + "Label", labelText, 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, false, 0f, 260f);
        CreateText(row.transform, valueName, "0", 22, FontStyles.Normal, TextAlignmentOptions.Left, Color.white, true);
    }

    private static void CreateDropdownRow(Transform parent, string rowName, string labelText, string dropdownName)
    {
        GameObject row = FindOrCreateChild(parent, rowName);
        ConfigureRowCard(row, 44f);
        CreateText(row.transform, rowName + "Label", labelText, 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, false, 0f, 260f);
        CreateDropdown(row.transform, dropdownName, 0f, 40f, true);
    }

    private static Canvas EnsureCanvas()
    {
        Canvas canvas = Object.FindObjectsOfType<Canvas>(true).FirstOrDefault(x => x.renderMode == RenderMode.ScreenSpaceOverlay);
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void ConfigureRowContainer(GameObject go, float height, float spacing)
    {
        RectTransform rect = EnsureRectTransform(go);
        rect.localScale = Vector3.one;

        LayoutElement layout = EnsureComponent<LayoutElement>(go);
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleWidth = 1f;
        layout.flexibleHeight = 0f;

        HorizontalLayoutGroup horizontal = EnsureComponent<HorizontalLayoutGroup>(go);
        horizontal.padding = new RectOffset(0, 0, 0, 0);
        horizontal.spacing = spacing;
        horizontal.childAlignment = TextAnchor.MiddleLeft;
        horizontal.childControlWidth = false;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;
    }

    private static void ConfigureCard(GameObject go, float minHeight)
    {
        RectTransform rect = EnsureRectTransform(go);
        rect.localScale = Vector3.one;

        LayoutElement layout = EnsureComponent<LayoutElement>(go);
        layout.minHeight = minHeight;
        layout.preferredHeight = minHeight;
        layout.flexibleWidth = 1f;

        Image image = EnsureComponent<Image>(go);
        image.color = new Color(1f, 1f, 1f, 0.08f);

        VerticalLayoutGroup vertical = EnsureComponent<VerticalLayoutGroup>(go);
        vertical.padding = new RectOffset(18, 18, 10, 10);
        vertical.spacing = 8f;
        vertical.childAlignment = TextAnchor.UpperLeft;
        vertical.childControlWidth = true;
        vertical.childControlHeight = false;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;
    }

    private static void ConfigureRowCard(GameObject go, float height)
    {
        ConfigureRowContainer(go, height, 12f);
        EnsureComponent<Image>(go).color = new Color(1f, 1f, 1f, 0.08f);
        HorizontalLayoutGroup horizontal = EnsureComponent<HorizontalLayoutGroup>(go);
        horizontal.padding = new RectOffset(18, 18, 8, 8);
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment, Color color, bool flexibleWidth, float preferredWidth = 0f, float minWidth = 0f)
    {
        GameObject go = FindOrCreateChild(parent, name);
        ConfigureTextObject(go, text, fontSize, style, alignment, color);

        LayoutElement layout = EnsureComponent<LayoutElement>(go);
        layout.minHeight = 30f;
        layout.flexibleWidth = flexibleWidth ? 1f : 0f;
        layout.preferredWidth = preferredWidth > 0f ? preferredWidth : -1f;
        layout.minWidth = minWidth > 0f ? minWidth : 0f;

        return go.GetComponent<TextMeshProUGUI>();
    }

    private static void ConfigureTextObject(GameObject go, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        TextMeshProUGUI tmp = EnsureComponent<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = false;
    }

    private static Button CreateButton(Transform parent, string name, string label, float width, float height, Color backgroundColor)
    {
        return CreateButton(parent, name, label, width, height, backgroundColor, Color.white);
    }

    private static Button CreateButton(Transform parent, string name, string label, float width, float height, Color backgroundColor, Color textColor)
    {
        GameObject go = FindOrCreateChild(parent, name);

        RectTransform rect = EnsureRectTransform(go);
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(go);
        image.color = backgroundColor;

        Button button = EnsureComponent<Button>(go);
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = backgroundColor * 1.05f;
        colors.pressedColor = backgroundColor * 0.9f;
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        LayoutElement layout = EnsureComponent<LayoutElement>(go);
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = height;
        layout.minHeight = height;
        layout.flexibleWidth = 0f;

        GameObject labelObject = FindOrCreateChild(go.transform, "Label");
        TextMeshProUGUI tmp = EnsureComponent<TextMeshProUGUI>(labelObject);
        tmp.text = label;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = textColor;

        RectTransform labelRect = EnsureRectTransform(labelObject);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private static Slider CreateSlider(Transform parent, string name, float width, float height)
    {
        GameObject root = FindOrCreateChild(parent, name);
        RectTransform rect = EnsureRectTransform(root);
        rect.localScale = Vector3.one;

        LayoutElement layout = EnsureComponent<LayoutElement>(root);
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = height;
        layout.minHeight = height;
        layout.flexibleWidth = 0f;

        Slider slider = EnsureComponent<Slider>(root);
        slider.minValue = 0f;
        slider.maxValue = 1000f;
        slider.wholeNumbers = true;
        slider.value = 100f;

        GameObject background = FindOrCreateChild(root.transform, "Background");
        Image backgroundImage = EnsureComponent<Image>(background);
        backgroundImage.color = new Color(0.28f, 0.28f, 0.28f, 1f);
        RectTransform backgroundRect = EnsureRectTransform(background);
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = FindOrCreateChild(root.transform, "Fill Area");
        RectTransform fillAreaRect = EnsureRectTransform(fillArea);
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(10f, 0f);
        fillAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = FindOrCreateChild(fillArea.transform, "Fill");
        Image fillImage = EnsureComponent<Image>(fill);
        fillImage.color = new Color(0.18f, 0.8f, 0.25f, 1f);
        RectTransform fillRect = EnsureRectTransform(fill);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleSlideArea = FindOrCreateChild(root.transform, "Handle Slide Area");
        RectTransform handleAreaRect = EnsureRectTransform(handleSlideArea);
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = FindOrCreateChild(handleSlideArea.transform, "Handle");
        Image handleImage = EnsureComponent<Image>(handle);
        handleImage.color = Color.white;
        RectTransform handleRect = EnsureRectTransform(handle);
        handleRect.sizeDelta = new Vector2(20f, 20f);

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static TMP_Dropdown CreateDropdown(Transform parent, string name, float width, float height)
    {
        return CreateDropdown(parent, name, width, height, false);
    }

    private static TMP_Dropdown CreateDropdown(Transform parent, string name, float width, float height, bool flexibleWidth)
    {
        GameObject root = FindOrCreateChild(parent, name);
        RectTransform rect = EnsureRectTransform(root);
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(root);
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        TMP_Dropdown dropdown = EnsureComponent<TMP_Dropdown>(root);
        dropdown.targetGraphic = image;
        if (dropdown.options.Count == 0)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData("Нет данных"));
        }
        dropdown.value = 0;

        LayoutElement layout = EnsureComponent<LayoutElement>(root);
        layout.preferredHeight = height;
        layout.minHeight = height;
        layout.flexibleWidth = flexibleWidth ? 1f : 0f;
        if (width > 0f)
        {
            layout.preferredWidth = width;
            layout.minWidth = width;
        }

        GameObject labelObject = FindOrCreateChild(root.transform, "Label");
        TextMeshProUGUI label = EnsureComponent<TextMeshProUGUI>(labelObject);
        label.text = "Нет данных";
        label.fontSize = 20f;
        label.color = Color.black;
        label.alignment = TextAlignmentOptions.Left;
        RectTransform labelRect = EnsureRectTransform(labelObject);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(14f, 6f);
        labelRect.offsetMax = new Vector2(-40f, -6f);
        dropdown.captionText = label;

        GameObject arrowObject = FindOrCreateChild(root.transform, "Arrow");
        TextMeshProUGUI arrow = EnsureComponent<TextMeshProUGUI>(arrowObject);
        arrow.text = "▼";
        arrow.fontSize = 20f;
        arrow.color = Color.black;
        arrow.alignment = TextAlignmentOptions.Center;
        RectTransform arrowRect = EnsureRectTransform(arrowObject);
        arrowRect.anchorMin = new Vector2(1f, 0.5f);
        arrowRect.anchorMax = new Vector2(1f, 0.5f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.sizeDelta = new Vector2(26f, 26f);
        arrowRect.anchoredPosition = new Vector2(-12f, 0f);

        GameObject template = FindOrCreateChild(root.transform, "Template");
        Image templateImage = EnsureComponent<Image>(template);
        templateImage.color = Color.white;
        ScrollRect scrollRect = EnsureComponent<ScrollRect>(template);
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        RectTransform templateRect = EnsureRectTransform(template);
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.sizeDelta = new Vector2(0f, 180f);
        templateRect.anchoredPosition = new Vector2(0f, -height);
        template.SetActive(false);

        GameObject viewport = FindOrCreateChild(template.transform, "Viewport");
        Mask mask = EnsureComponent<Mask>(viewport);
        mask.showMaskGraphic = false;
        Image viewportImage = EnsureComponent<Image>(viewport);
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        RectTransform viewportRect = EnsureRectTransform(viewport);
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        GameObject content = FindOrCreateChild(viewport.transform, "Content");
        RectTransform contentRect = EnsureRectTransform(content);
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 28f);
        VerticalLayoutGroup contentLayout = EnsureComponent<VerticalLayoutGroup>(content);
        contentLayout.padding = new RectOffset(0, 0, 0, 0);
        contentLayout.spacing = 0f;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter contentFitter = EnsureComponent<ContentSizeFitter>(content);
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject item = FindOrCreateChild(content.transform, "Item");
        item.transform.SetParent(template.transform, false);
        Toggle toggle = EnsureComponent<Toggle>(item);
        Image itemImage = EnsureComponent<Image>(item);
        itemImage.color = new Color(1f, 1f, 1f, 0.95f);
        RectTransform itemRect = EnsureRectTransform(item);
        itemRect.anchorMin = new Vector2(0f, 1f);
        itemRect.anchorMax = new Vector2(1f, 1f);
        itemRect.pivot = new Vector2(0.5f, 1f);
        itemRect.sizeDelta = new Vector2(0f, 28f);

        GameObject itemCheckmark = FindOrCreateChild(item.transform, "Item Checkmark");
        Image itemCheckmarkImage = EnsureComponent<Image>(itemCheckmark);
        itemCheckmarkImage.color = new Color(0.18f, 0.8f, 0.25f, 1f);
        RectTransform itemCheckmarkRect = EnsureRectTransform(itemCheckmark);
        itemCheckmarkRect.anchorMin = new Vector2(0f, 0.5f);
        itemCheckmarkRect.anchorMax = new Vector2(0f, 0.5f);
        itemCheckmarkRect.pivot = new Vector2(0f, 0.5f);
        itemCheckmarkRect.sizeDelta = new Vector2(18f, 18f);
        itemCheckmarkRect.anchoredPosition = new Vector2(10f, 0f);

        GameObject itemLabelObject = FindOrCreateChild(item.transform, "Item Label");
        TextMeshProUGUI itemLabel = EnsureComponent<TextMeshProUGUI>(itemLabelObject);
        itemLabel.text = "Option";
        itemLabel.fontSize = 18f;
        itemLabel.color = Color.black;
        itemLabel.alignment = TextAlignmentOptions.Left;
        RectTransform itemLabelRect = EnsureRectTransform(itemLabelObject);
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(36f, 3f);
        itemLabelRect.offsetMax = new Vector2(-10f, -3f);

        toggle.targetGraphic = itemImage;
        toggle.graphic = itemCheckmarkImage;
        dropdown.template = templateRect;
        dropdown.itemText = itemLabel;
        dropdown.itemImage = itemImage;
        dropdown.captionText = label;
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        return dropdown;
    }

    private static RectTransform EnsureRectTransform(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = go.AddComponent<RectTransform>();
        }
        return rect;
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = Undo.AddComponent<T>(go);
        }
        return component;
    }
}
