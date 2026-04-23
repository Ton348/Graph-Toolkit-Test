using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class TraderShopWindowBuilder
{
    private const string MenuPath = "Tools/Business/Build Trader Shop UI";
    private const string WindowName = "TraderShopWindow";

    [MenuItem(MenuPath)]
    public static void BuildTraderShopUi()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();

        GameObject window = FindOrCreateChild(canvas.transform, WindowName);
        ConfigureWindow(window);

        GameObject dimmer = FindOrCreateChild(window.transform, "Dimmer");
        ConfigureDimmer(dimmer);

        GameObject panel = FindOrCreateChild(window.transform, "Panel");
        ConfigurePanel(panel);

        GameObject header = FindOrCreateChild(panel.transform, "Header");
        ConfigureHeader(header);

        GameObject body = FindOrCreateChild(panel.transform, "Body");
        ConfigureBody(body);

        GameObject footer = FindOrCreateChild(panel.transform, "Footer");
        ConfigureFooter(footer);

        Selection.activeGameObject = window;
        EditorUtility.SetDirty(window);
        EditorUtility.SetDirty(canvas.gameObject);
        EditorSceneManager.MarkSceneDirty(window.scene);

        Debug.Log("[TraderShopWindowBuilder] Trader shop UI created/updated.");
    }

    private static void ConfigureWindow(GameObject window)
    {
        RectTransform rect = EnsureRectTransform(window);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(window);
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = false;

        CanvasGroup canvasGroup = EnsureComponent<CanvasGroup>(window);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private static void ConfigureDimmer(GameObject dimmer)
    {
        RectTransform rect = EnsureRectTransform(dimmer);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(dimmer);
        image.color = new Color(0f, 0f, 0f, 0.55f);
        image.raycastTarget = true;
    }

    private static void ConfigurePanel(GameObject panel)
    {
        RectTransform rect = EnsureRectTransform(panel);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(980f, 720f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(panel);
        image.color = new Color(0.11f, 0.13f, 0.18f, 0.98f);
        image.raycastTarget = true;

        VerticalLayoutGroup layout = EnsureComponent<VerticalLayoutGroup>(panel);
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(panel);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private static void ConfigureHeader(GameObject header)
    {
        ConfigureRowContainer(header, 86f, 12f);

        GameObject left = FindOrCreateChild(header.transform, "Left");
        RectTransform leftRect = EnsureRectTransform(left);
        leftRect.localScale = Vector3.one;

        LayoutElement leftLayout = EnsureComponent<LayoutElement>(left);
        leftLayout.flexibleWidth = 1f;
        leftLayout.minHeight = 86f;

        VerticalLayoutGroup leftGroup = EnsureComponent<VerticalLayoutGroup>(left);
        leftGroup.padding = new RectOffset(0, 0, 0, 0);
        leftGroup.spacing = 4f;
        leftGroup.childAlignment = TextAnchor.UpperLeft;
        leftGroup.childControlWidth = true;
        leftGroup.childControlHeight = false;
        leftGroup.childForceExpandWidth = true;
        leftGroup.childForceExpandHeight = false;

        CreateText(left.transform, "TitleText", "Магазин торговца", 30, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, true);
        CreateText(left.transform, "TraderNameText", "Торговец", 20, FontStyles.Normal, TextAlignmentOptions.Left, new Color(0.82f, 0.86f, 0.95f), true);

        CreateButton(header.transform, "CloseButton", "X", 60f, 48f, new Color(0.42f, 0.42f, 0.42f, 1f));
    }

    private static void ConfigureBody(GameObject body)
    {
        RectTransform rect = EnsureRectTransform(body);
        rect.localScale = Vector3.one;

        LayoutElement layout = EnsureComponent<LayoutElement>(body);
        layout.flexibleHeight = 1f;
        layout.minHeight = 540f;
        layout.flexibleWidth = 1f;

        Image image = EnsureComponent<Image>(body);
        image.color = new Color(1f, 1f, 1f, 0.03f);
        image.raycastTarget = false;

        VerticalLayoutGroup group = EnsureComponent<VerticalLayoutGroup>(body);
        group.padding = new RectOffset(0, 0, 0, 0);
        group.spacing = 10f;
        group.childAlignment = TextAnchor.UpperLeft;
        group.childControlWidth = true;
        group.childControlHeight = false;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        GameObject scrollView = FindOrCreateChild(body.transform, "ItemListScrollView");
        ConfigureScrollView(scrollView);
    }

    private static void ConfigureFooter(GameObject footer)
    {
        ConfigureRowContainer(footer, 42f, 12f);
        CreateText(
            footer.transform,
            "StatusText",
            "Выберите товар.",
            20,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            new Color(0.90f, 0.90f, 0.90f),
            true);
    }

    private static void ConfigureScrollView(GameObject scrollView)
    {
        RectTransform rect = EnsureRectTransform(scrollView);
        rect.localScale = Vector3.one;

        LayoutElement layout = EnsureComponent<LayoutElement>(scrollView);
        layout.flexibleHeight = 1f;
        layout.minHeight = 520f;
        layout.flexibleWidth = 1f;

        Image image = EnsureComponent<Image>(scrollView);
        image.color = new Color(1f, 1f, 1f, 0.015f);
        image.raycastTarget = false;

        ScrollRect scrollRect = EnsureComponent<ScrollRect>(scrollView);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.inertia = true;

        GameObject viewport = FindOrCreateChild(scrollView.transform, "Viewport");
        RectTransform viewportRect = EnsureRectTransform(viewport);
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-18f, 0f);
        viewportRect.localScale = Vector3.one;

        Image viewportImage = EnsureComponent<Image>(viewport);
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        viewportImage.raycastTarget = true;

        Mask mask = EnsureComponent<Mask>(viewport);
        mask.showMaskGraphic = false;

        GameObject content = FindOrCreateChild(viewport.transform, "Content");
        RectTransform contentRect = EnsureRectTransform(content);
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);
        contentRect.localScale = Vector3.one;

        Image contentImage = EnsureComponent<Image>(content);
        contentImage.color = new Color(0f, 0f, 0f, 0f);
        contentImage.raycastTarget = false;

        VerticalLayoutGroup contentGroup = EnsureComponent<VerticalLayoutGroup>(content);
        contentGroup.padding = new RectOffset(0, 0, 0, 0);
        contentGroup.spacing = 12f;
        contentGroup.childAlignment = TextAnchor.UpperLeft;
        contentGroup.childControlWidth = true;
        contentGroup.childControlHeight = false;
        contentGroup.childForceExpandWidth = true;
        contentGroup.childForceExpandHeight = false;

        ContentSizeFitter contentFitter = EnsureComponent<ContentSizeFitter>(content);
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject scrollbar = FindOrCreateChild(scrollView.transform, "Scrollbar Vertical");
        ConfigureVerticalScrollbar(scrollbar);

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.verticalScrollbar = scrollbar.GetComponent<Scrollbar>();
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 8f;

        GameObject template = FindOrCreateChild(content.transform, "ItemCardTemplate");
        ConfigureItemCardTemplate(template);
        template.SetActive(false);
    }

    private static void ConfigureVerticalScrollbar(GameObject scrollbar)
    {
        RectTransform rect = EnsureRectTransform(scrollbar);
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(18f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = new Vector2(-18f, 0f);
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        Image background = EnsureComponent<Image>(scrollbar);
        background.color = new Color(1f, 1f, 1f, 0.08f);
        background.raycastTarget = true;

        Scrollbar bar = EnsureComponent<Scrollbar>(scrollbar);
        bar.direction = Scrollbar.Direction.BottomToTop;
        bar.numberOfSteps = 0;
        bar.size = 0.2f;

        GameObject slidingArea = FindOrCreateChild(scrollbar.transform, "Sliding Area");
        RectTransform slidingRect = EnsureRectTransform(slidingArea);
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = new Vector2(3f, 3f);
        slidingRect.offsetMax = new Vector2(-3f, -3f);
        slidingRect.localScale = Vector3.one;

        Image slidingImage = EnsureComponent<Image>(slidingArea);
        slidingImage.color = new Color(0f, 0f, 0f, 0f);
        slidingImage.raycastTarget = false;

        GameObject handle = FindOrCreateChild(slidingArea.transform, "Handle");
        RectTransform handleRect = EnsureRectTransform(handle);
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(1f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(0f, 80f);
        handleRect.localScale = Vector3.one;

        Image handleImage = EnsureComponent<Image>(handle);
        handleImage.color = new Color(0.72f, 0.72f, 0.72f, 1f);
        handleImage.raycastTarget = true;

        bar.targetGraphic = handleImage;
        bar.handleRect = handleRect;
    }

    private static void ConfigureItemCardTemplate(GameObject card)
    {
        RectTransform rect = EnsureRectTransform(card);
        rect.localScale = Vector3.one;

        LayoutElement layout = EnsureComponent<LayoutElement>(card);
        layout.flexibleWidth = 1f;
        layout.minHeight = 176f;
        layout.preferredHeight = 176f;

        Image image = EnsureComponent<Image>(card);
        image.color = new Color(1f, 1f, 1f, 0.08f);
        image.raycastTarget = false;

        VerticalLayoutGroup group = EnsureComponent<VerticalLayoutGroup>(card);
        group.padding = new RectOffset(16, 16, 14, 14);
        group.spacing = 8f;
        group.childAlignment = TextAnchor.UpperLeft;
        group.childControlWidth = true;
        group.childControlHeight = false;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        CreateText(card.transform, "ItemNameText", "Название товара", 24, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, true);
        CreateText(card.transform, "ItemDescriptionText", "Описание товара", 18, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.86f, 0.90f, 0.96f), true);
        CreateText(card.transform, "ItemStatsText", "Характеристики", 18, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.75f, 0.92f, 0.78f), true);

        GameObject bottom = FindOrCreateChild(card.transform, "BottomRow");
        ConfigureRowContainer(bottom, 44f, 12f);

        CreateText(bottom.transform, "PriceText", "Цена: 0", 22, FontStyles.Bold, TextAlignmentOptions.Left, new Color(1f, 0.95f, 0.55f), true);
        CreateButton(bottom.transform, "BuyButton", "Купить", 150f, 40f, new Color(0.17f, 0.8f, 0.25f, 1f));
    }

    private static Canvas EnsureCanvas()
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvas;
            }
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

        Canvas newCanvas = canvasObject.GetComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return newCanvas;
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

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        string text,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color,
        bool flexibleWidth)
    {
        GameObject go = FindOrCreateChild(parent, name);
        RectTransform rect = EnsureRectTransform(go);
        rect.localScale = Vector3.one;

        TextMeshProUGUI tmp = EnsureComponent<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        LayoutElement layout = EnsureComponent<LayoutElement>(go);
        layout.minHeight = 24f;
        layout.flexibleWidth = flexibleWidth ? 1f : 0f;

        return tmp;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        float width,
        float height,
        Color backgroundColor)
    {
        GameObject go = FindOrCreateChild(parent, name);
        RectTransform rect = EnsureRectTransform(go);
        rect.localScale = Vector3.one;

        Image image = EnsureComponent<Image>(go);
        image.color = backgroundColor;
        image.raycastTarget = true;

        Button button = EnsureComponent<Button>(go);
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = backgroundColor * 1.05f;
        colors.pressedColor = backgroundColor * 0.9f;
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.targetGraphic = image;

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
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        RectTransform labelRect = EnsureRectTransform(labelObject);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        labelRect.localScale = Vector3.one;

        return button;
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