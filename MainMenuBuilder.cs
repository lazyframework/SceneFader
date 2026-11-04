using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MainMenuBuilder : EditorWindow
{
    Sprite background, character;
    TMP_FontAsset customFont;
    string gameScene = "GameScene";
    string shopScene = "ShopScene";

    [MenuItem("Tools/Bangun Main Menu")]
    static void Open() => GetWindow<MainMenuBuilder>("Main Menu Builder");

    void OnGUI()
    {
        GUILayout.Label("Aset Menu Futuristik", EditorStyles.boldLabel);
        background = (Sprite)EditorGUILayout.ObjectField("Background", background, typeof(Sprite), false);
        character  = (Sprite)EditorGUILayout.ObjectField("Karakter", character, typeof(Sprite), false);
        customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font TMP (Sci-Fi)", customFont, typeof(TMP_FontAsset), false);
        
        gameScene  = EditorGUILayout.TextField("Nama Scene Game", gameScene);
        shopScene  = EditorGUILayout.TextField("Nama Scene Shop", shopScene);

        GUILayout.Space(10);
        if (GUILayout.Button("BANGUN MENU FUTURISTIK", GUILayout.Height(40))) Build();
    }

    void Build()
    {
        if (GameObject.Find("MenuManager") != null)
        {
            EditorUtility.DisplayDialog("Sudah ada", "Hapus Canvas dan MenuManager lama dulu.", "OK");
            return;
        }

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem (INPUT LAMA / STandalone)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Background
        var bg = NewImage("Background", canvasGO.transform, background, new Color(0.05f, 0.05f, 0.1f));
        bg.raycastTarget = false;
        bg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        bg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        bg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        bg.rectTransform.anchoredPosition = Vector2.zero;
        bg.rectTransform.sizeDelta = new Vector2(100, 100);
        bg.gameObject.AddComponent<AutoFitBackground>(); 
        bg.gameObject.AddComponent<BackgroundAnimator>(); 

        // Karakter
        var chr = NewImage("Karakter", canvasGO.transform, character, Color.white);
        chr.preserveAspect = true;
        chr.raycastTarget = false;
        chr.enabled = character != null;
        var cr = chr.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0f);
        cr.anchoredPosition = Vector2.zero;
        cr.sizeDelta = new Vector2(1000, 1080);

        // Manager
        var menu = new GameObject("MenuManager").AddComponent<MainMenu>();

        // Panel Settings
        var panel = NewImage("PanelSettings", canvasGO.transform, null, new Color(0.02f, 0.05f, 0.1f, 0.95f));
        Stretch(panel.rectTransform);
        NewText("SettingsTitle", panel.transform, "SYSTEM CONFIG", 48).rectTransform.anchoredPosition = new Vector2(0, 150);
        
        var volLabel = NewText("VolumeLabel", panel.transform, "AUDIO VOLUME", 24);
        volLabel.rectTransform.anchoredPosition = new Vector2(0, 60);
        volLabel.color = new Color(0.2f, 0.6f, 1f);

        var sliderGO = NewSlider(panel.transform);
        sliderGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        var settingsPanel = panel.gameObject.AddComponent<SettingsPanel>();
        var spSO = new SerializedObject(settingsPanel);
        spSO.FindProperty("volumeSlider").objectReferenceValue = sliderGO.GetComponent<Slider>();
        spSO.ApplyModifiedProperties();

        var back = NewFuturisticButton("BtnBack", panel.transform, "BACK TO MENU", new Vector2(0, -150), new Vector2(300, 60), TextAlignmentOptions.Center);
        UnityEventTools.AddPersistentListener(back.onClick, menu.CloseSettings);
        panel.gameObject.SetActive(false);

        // Tombol utama
        float startX = -150; 
        float startY = 150;

        var start = NewFuturisticButton("BtnStart", canvasGO.transform, "START GAME", new Vector2(startX, startY), new Vector2(320, 70), TextAlignmentOptions.Center);
        var shop  = NewFuturisticButton("BtnShop", canvasGO.transform, "SHOP", new Vector2(startX, startY - 90), new Vector2(320, 70), TextAlignmentOptions.Center);
        var set   = NewFuturisticButton("BtnSettings", canvasGO.transform, "SETTINGS", new Vector2(startX, startY - 180), new Vector2(320, 70), TextAlignmentOptions.Center);
        var quit  = NewFuturisticButton("BtnQuit", canvasGO.transform, "QUIT", new Vector2(startX, startY - 270), new Vector2(320, 70), TextAlignmentOptions.Center);

        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(shop.onClick,  menu.OpenShop);
        UnityEventTools.AddPersistentListener(set.onClick,   menu.OpenSettings);
        UnityEventTools.AddPersistentListener(quit.onClick,  menu.QuitGame);

        AddAnimation(start.gameObject, 0.1f);
        AddAnimation(shop.gameObject, 0.2f);
        AddAnimation(set.gameObject, 0.3f);
        AddAnimation(quit.gameObject, 0.4f);

        panel.transform.SetAsLastSibling();

        var so = new SerializedObject(menu);
        so.FindProperty("panelSettings").objectReferenceValue = panel.gameObject;
        so.FindProperty("gameSceneName").stringValue = gameScene;
        so.FindProperty("shopSceneName").stringValue = shopScene;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
    }

    // --- FUNGSI TOMBOL FUTURISTIK ---
    Button NewFuturisticButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size, TextAlignmentOptions align)
    {
        var btnObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        btnObj.transform.SetParent(parent, false);
        
        var rt = btnObj.GetComponent<RectTransform>();
        
        if (parent.name == "PanelSettings")
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
        }
        
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        rt.localRotation = Quaternion.Euler(0, 0, -10);

        var bgImage = btnObj.GetComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.8f);

        var outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.8f, 1f, 0.8f); 
        outline.effectDistance = new Vector2(2f, 2f);

        var accent = NewImage("Accent", btnObj.transform, null, new Color(0.2f, 0.8f, 1f));
        var aRt = accent.rectTransform;
        aRt.anchorMin = aRt.anchorMax = new Vector2(0f, 0.5f);
        aRt.pivot = new Vector2(0f, 0.5f);
        aRt.anchoredPosition = new Vector2(-10, 0);
        aRt.sizeDelta = new Vector2(20, 4);

        var txt = NewText("Label", btnObj.transform, label, 32);
        txt.fontStyle = FontStyles.Bold;
        Stretch(txt.rectTransform);
        txt.rectTransform.localRotation = Quaternion.Euler(0, 0, 10);
        txt.alignment = align;

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.2f, 0.8f, 1f, 0.4f); 
        colors.pressedColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        return btn;
    }

    // --- HELPER ---
    Image NewImage(string name, Transform parent, Sprite s, Color c)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = s;
        img.color = s != null ? Color.white : c;
        return img;
    }

    TextMeshProUGUI NewText(string name, Transform parent, string text, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        if (customFont != null) t.font = customFont;
        return t;
    }

    void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    GameObject NewSlider(Transform parent)
    {
        var root = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 20);

        var back = NewImage("Background", root.transform, null, new Color(0.2f, 0.3f, 0.4f, 0.5f));
        Stretch(back.rectTransform);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());
        var fill = NewImage("Fill", fillArea.transform, null, new Color(0.2f, 0.8f, 1f)); 
        Stretch(fill.rectTransform);

        var handleArea = new GameObject("Handle Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>());
        var handle = NewImage("Handle", handleArea.transform, null, Color.white);
        handle.rectTransform.sizeDelta = new Vector2(15, 30);

        var s = root.GetComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.minValue = 0;
        s.maxValue = 1;
        return root;
    }

    void AddAnimation(GameObject btnObj, float delay)
    {
        var anim = btnObj.AddComponent<MenuAnimator>();
        anim.delay = delay;
    }
}
