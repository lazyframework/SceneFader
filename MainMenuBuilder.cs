using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI; 
using TMPro;

public class MainMenuBuilder : EditorWindow
{
    Sprite background, character, diamond;
    string gameScene = "GameScene";
    string shopScene = "ShopScene";

    [MenuItem("Tools/Bangun Main Menu")]
    static void Open() => GetWindow<MainMenuBuilder>("Main Menu Builder");

    void OnGUI()
    {
        GUILayout.Label("Aset (boleh dikosongkan)", EditorStyles.boldLabel);
        background = (Sprite)EditorGUILayout.ObjectField("Background", background, typeof(Sprite), false);
        character  = (Sprite)EditorGUILayout.ObjectField("Karakter", character, typeof(Sprite), false);
        diamond    = (Sprite)EditorGUILayout.ObjectField("Sprite Tombol", diamond, typeof(Sprite), false);
        gameScene  = EditorGUILayout.TextField("Nama Scene Game", gameScene);
        shopScene  = EditorGUILayout.TextField("Nama Scene Shop", shopScene);

        GUILayout.Space(10);
        if (GUILayout.Button("BANGUN MENU", GUILayout.Height(35))) Build();
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

        // EventSystem (MURNI INPUT SYSTEM BARU)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
        }

        // Background
        var bg = NewImage("Background", canvasGO.transform, background, new Color(0.1f, 0.1f, 0.2f));
        bg.raycastTarget = false;
        Stretch(bg.rectTransform);
        bg.rectTransform.sizeDelta = new Vector2(100, 100); 
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
        var panel = NewImage("PanelSettings", canvasGO.transform, null, new Color(0, 0, 0, 0.85f));
        Stretch(panel.rectTransform);
        NewText("SettingsTitle", panel.transform, "Settings", 48).rectTransform.anchoredPosition = new Vector2(0, 150);
        NewText("VolumeLabel", panel.transform, "Volume", 32).rectTransform.anchoredPosition = new Vector2(0, 40);

        var sliderGO = NewSlider(panel.transform);
        sliderGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);

        var settingsPanel = panel.gameObject.AddComponent<SettingsPanel>();
        var spSO = new SerializedObject(settingsPanel);
        spSO.FindProperty("volumeSlider").objectReferenceValue = sliderGO.GetComponent<Slider>();
        spSO.ApplyModifiedProperties();

        var back = NewBox("BtnBack", panel.transform, "Back", new Vector2(0, -150), 260);
        UnityEventTools.AddPersistentListener(back.onClick, menu.CloseSettings);
        panel.gameObject.SetActive(false);

        // Tombol utama
        var start = NewDiamond("BtnStart",    canvasGO.transform, "START",    new Vector2(1, 0), new Vector2(-193, 220), 200);
        var shop  = NewDiamond("BtnShop",     canvasGO.transform, "SHOP",     new Vector2(1, 0), new Vector2(-270, 365), 100);
        var set   = NewDiamond("BtnSettings", canvasGO.transform, "SETTINGS", new Vector2(0, 0), new Vector2(220, 230),  200);
        var quit  = NewDiamond("BtnQuit",     canvasGO.transform, "QUIT",     new Vector2(0, 0), new Vector2(373, 160),  100);

        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(shop.onClick,  menu.OpenShop);
        UnityEventTools.AddPersistentListener(set.onClick,   menu.OpenSettings);
        UnityEventTools.AddPersistentListener(quit.onClick,  menu.QuitGame);

        AddAnimation(start.gameObject, 0.2f);
        AddAnimation(shop.gameObject, 0.4f);
        AddAnimation(set.gameObject, 0.6f);
        AddAnimation(quit.gameObject, 0.8f);

        panel.transform.SetAsLastSibling();

        var so = new SerializedObject(menu);
        so.FindProperty("panelSettings").objectReferenceValue = panel.gameObject;
        so.FindProperty("gameSceneName").stringValue = gameScene;
        so.FindProperty("shopSceneName").stringValue = shopScene;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
    }

    // ---------- Helper ----------
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
        t.rectTransform.sizeDelta = new Vector2(600, 100);
        return t;
    }

    void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    Button NewDiamond(string name, Transform parent, string label, Vector2 anchor, Vector2 pos, float size)
    {
        var img = NewImage(name, parent, diamond, new Color(0.04f, 0.04f, 0.07f, 0.92f));
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);
        rt.localRotation = Quaternion.Euler(0, 0, 45);

        var outline = img.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.6f);
        outline.effectDistance = new Vector2(3, 3);

        var btn = img.gameObject.AddComponent<Button>();
        Colorize(btn);

        var txt = NewText("Label", img.transform, label, size * 0.14f);
        Stretch(txt.rectTransform);
        txt.rectTransform.localRotation = Quaternion.Euler(0, 0, -45);
        return btn;
    }

    Button NewBox(string name, Transform parent, string label, Vector2 pos, float w)
    {
        var img = NewImage(name, parent, null, new Color(0.05f, 0.05f, 0.1f, 0.95f));
        var rt = img.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(w, 70);
        var btn = img.gameObject.AddComponent<Button>();
        Colorize(btn);
        var txt = NewText("Label", img.transform, label, 30);
        Stretch(txt.rectTransform);
        return btn;
    }

    GameObject NewSlider(Transform parent)
    {
        var root = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 30);

        var back = NewImage("Background", root.transform, null, new Color(1, 1, 1, 0.2f));
        Stretch(back.rectTransform);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());
        var fill = NewImage("Fill", fillArea.transform, null, new Color(0.3f, 0.8f, 1f));
        Stretch(fill.rectTransform);

        var handleArea = new GameObject("Handle Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>());
        var handle = NewImage("Handle", handleArea.transform, null, Color.white);
        handle.rectTransform.sizeDelta = new Vector2(24, 40);

        var s = root.GetComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.minValue = 0;
        s.maxValue = 1;
        return root;
    }

    void Colorize(Button b)
    {
        var c = b.colors;
        c.normalColor = Color.white;
        c.highlightedColor = new Color(0.6f, 0.9f, 1f);
        c.pressedColor = new Color(0.4f, 0.6f, 0.8f);
        c.fadeDuration = 0.1f;
        b.colors = c;
    }

    void AddAnimation(GameObject btnObj, float delay)
    {
        var anim = btnObj.AddComponent<MenuAnimator>();
        anim.delay = delay;
    }
}
