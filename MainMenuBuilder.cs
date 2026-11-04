using System.IO;
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
    string gameTitle = "ETERNAL WORLD";
    string subtitle  = "ADVENTURE AWAITS";
    string version   = "v1.0.0";
    Vector2 scrollPos;

    static readonly Color Cyan   = new Color(0.2f, 0.8f, 1f);
    static readonly Color Gold   = new Color(1f, 0.82f, 0.2f);
    static readonly Color Red    = new Color(1f, 0.3f, 0.35f);
    static readonly Color Glass  = new Color(0f, 0f, 0f, 0.55f);
    static readonly Color Panel  = new Color(0.02f, 0.06f, 0.12f, 0.95f);

    Sprite slantSprite, slantOutline, cutSprite, cutOutline, gradientSprite;

    [MenuItem("Tools/Bangun Main Menu")]
    static void Open() => GetWindow<MainMenuBuilder>("Main Menu Builder");

    void OnGUI()
    {
        GUILayout.Space(6);
        if (GUILayout.Button("BANGUN MENU FUTURISTIK", GUILayout.Height(40))) Build();
        GUILayout.Space(6);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Aset Menu Futuristik", EditorStyles.boldLabel);
        background = (Sprite)EditorGUILayout.ObjectField("Background", background, typeof(Sprite), false);
        character  = (Sprite)EditorGUILayout.ObjectField("Karakter", character, typeof(Sprite), false);
        customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font TMP (Sci-Fi)", customFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(6);
        GUILayout.Label("Teks & Scene", EditorStyles.boldLabel);
        gameTitle = EditorGUILayout.TextField("Judul game", gameTitle);
        subtitle  = EditorGUILayout.TextField("Subjudul", subtitle);
        version   = EditorGUILayout.TextField("Versi", version);
        gameScene = EditorGUILayout.TextField("Nama Scene Game", gameScene);
        shopScene = EditorGUILayout.TextField("Nama Scene Shop", shopScene);
        EditorGUILayout.EndScrollView();
    }

    void Build()
    {
        if (GameObject.Find("MenuManager") != null)
        {
            EditorUtility.DisplayDialog("Sudah ada", "Hapus Canvas dan MenuManager lama dulu.", "OK");
            return;
        }

        CreateSprites();

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGO.transform;

        // EventSystem (input lama / Standalone)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Background (tetap memakai komponen lama)
        var bg = NewImage("Background", root, background, new Color(0.05f, 0.05f, 0.1f));
        bg.raycastTarget = false;
        Place(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 100));
        bg.gameObject.AddComponent<AutoFitBackground>();
        bg.gameObject.AddComponent<BackgroundAnimator>();

        // Karakter di sisi kanan
        var chr = NewImage("Karakter", root, character, Color.white);
        chr.preserveAspect = true;
        chr.raycastTarget = false;
        chr.enabled = character != null;
        Place(chr.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(330, 0), new Vector2(1000, 1080));

        // Gradasi gelap di kiri supaya tombol terbaca
        var grad = NewImage("LeftShade", root, gradientSprite, Color.white);
        grad.raycastTarget = false;
        var gr = grad.rectTransform;
        gr.anchorMin = new Vector2(0, 0);
        gr.anchorMax = new Vector2(0, 1);
        gr.pivot = new Vector2(0, 0.5f);
        gr.anchoredPosition = Vector2.zero;
        gr.sizeDelta = new Vector2(1100, 0);

        // Manager
        var menu = new GameObject("MenuManager").AddComponent<MainMenu>();

        // ---------- Judul ----------
        var title = NewText("Title", root, gameTitle, 96);
        title.fontStyle = FontStyles.Bold | FontStyles.Italic;
        title.alignment = TextAlignmentOptions.Left;
        title.characterSpacing = 4;
        Place(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(110, -70), new Vector2(1000, 120));

        var sub = NewText("Subtitle", root, subtitle, 28);
        sub.alignment = TextAlignmentOptions.Left;
        sub.color = Cyan;
        sub.characterSpacing = 18;
        Place(sub.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(116, -190), new Vector2(900, 40));

        var line = NewImage("TitleLine", root, slantSprite, Cyan);
        line.type = Image.Type.Sliced;
        line.raycastTarget = false;
        Place(line.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(110, -240), new Vector2(420, 8));

        // ---------- Tombol utama (bertingkat ke kanan) ----------
        var L = new Vector2(0, 0.5f);
        var start = MakeButton(root, "BtnStart",    "01", "START GAME", L, new Vector2(110, 60),   new Vector2(500, 100), Cyan, true);
        var shop  = MakeButton(root, "BtnShop",     "02", "SHOP",       L, new Vector2(150, -55),  new Vector2(420, 78),  Gold, false);
        var set   = MakeButton(root, "BtnSettings", "03", "SETTINGS",   L, new Vector2(190, -145), new Vector2(420, 78),  Cyan, false);
        var quit  = MakeButton(root, "BtnQuit",     "04", "QUIT",       L, new Vector2(230, -235), new Vector2(420, 78),  Red,  false);

        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(shop.onClick,  menu.OpenShop);
        UnityEventTools.AddPersistentListener(set.onClick,   menu.OpenSettings);
        UnityEventTools.AddPersistentListener(quit.onClick,  menu.QuitGame);

        AddAnimation(start.gameObject, 0.1f);
        AddAnimation(shop.gameObject, 0.2f);
        AddAnimation(set.gameObject, 0.3f);
        AddAnimation(quit.gameObject, 0.4f);

        // Versi
        var ver = NewText("Version", root, version, 22);
        ver.alignment = TextAlignmentOptions.Left;
        ver.color = new Color(1, 1, 1, 0.6f);
        Place(ver.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(110, 40), new Vector2(300, 34));

        // ---------- Panel Settings ----------
        var panel = NewImage("PanelSettings", root, null, new Color(0f, 0f, 0f, 0.8f));
        Stretch(panel.rectTransform);

        var card = NewImage("Card", panel.transform, cutSprite, Panel);
        card.type = Image.Type.Sliced;
        Place(card.rectTransform, MidV, MidV, Vector2.zero, new Vector2(900, 560));
        var cardBorder = NewImage("Border", card.transform, cutOutline, Cyan);
        cardBorder.type = Image.Type.Sliced;
        cardBorder.raycastTarget = false;
        Stretch(cardBorder.rectTransform);

        var sTitle = NewText("SettingsTitle", card.transform, "SYSTEM CONFIG", 52);
        sTitle.fontStyle = FontStyles.Bold | FontStyles.Italic;
        Place(sTitle.rectTransform, MidV, MidV, new Vector2(0, 190), new Vector2(800, 70));

        var sLine = NewImage("Line", card.transform, slantSprite, Cyan);
        sLine.type = Image.Type.Sliced;
        sLine.raycastTarget = false;
        Place(sLine.rectTransform, MidV, MidV, new Vector2(0, 145), new Vector2(300, 6));

        var volLabel = NewText("VolumeLabel", card.transform, "AUDIO VOLUME", 26);
        volLabel.color = Cyan;
        volLabel.characterSpacing = 10;
        Place(volLabel.rectTransform, MidV, MidV, new Vector2(0, 60), new Vector2(600, 40));

        var sliderGO = NewSlider(card.transform);
        sliderGO.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        var settingsPanel = panel.gameObject.AddComponent<SettingsPanel>();
        var spSO = new SerializedObject(settingsPanel);
        spSO.FindProperty("volumeSlider").objectReferenceValue = sliderGO.GetComponent<Slider>();
        spSO.ApplyModifiedProperties();

        var back = MakeButton(card.transform, "BtnBack", "<", "BACK TO MENU", MidV, new Vector2(0, -170), new Vector2(420, 78), Cyan, false);
        UnityEventTools.AddPersistentListener(back.onClick, menu.CloseSettings);
        panel.gameObject.SetActive(false);
        panel.transform.SetAsLastSibling();

        var so = new SerializedObject(menu);
        so.FindProperty("panelSettings").objectReferenceValue = panel.gameObject;
        so.FindProperty("gameSceneName").stringValue = gameScene;
        so.FindProperty("shopSceneName").stringValue = shopScene;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
    }

    static readonly Vector2 MidV = new Vector2(0.5f, 0.5f);

    // ---------- Tombol futuristik: jajar genjang, garis tepi neon, nomor, aksen, panah ----------
    Button MakeButton(Transform parent, string name, string tag, string label, Vector2 anchor,
                      Vector2 pos, Vector2 size, Color accent, bool primary)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), anchor, anchor, pos, size);

        var bg = go.GetComponent<Image>();
        bg.sprite = slantSprite;
        bg.type = Image.Type.Sliced;
        bg.color = Color.white; // warna diatur lewat Button.colors

        var border = NewImage("Outline", go.transform, slantOutline, accent);
        border.type = Image.Type.Sliced;
        border.raycastTarget = false;
        Stretch(border.rectTransform);

        // Garis aksen miring di kiri
        var bar = NewImage("Accent", go.transform, null, accent);
        bar.raycastTarget = false;
        Place(bar.rectTransform, new Vector2(0, 0.5f), MidV, new Vector2(36, 0), new Vector2(6, size.y * 0.5f));
        bar.rectTransform.localRotation = Quaternion.Euler(0, 0, -17);

        var num = NewText("Tag", go.transform, tag, 22);
        num.alignment = TextAlignmentOptions.Left;
        num.color = accent;
        num.fontStyle = FontStyles.Bold;
        Place(num.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(56, 0), new Vector2(50, 34));

        var txt = NewText("Label", go.transform, label, primary ? 42 : 32);
        txt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        txt.alignment = TextAlignmentOptions.Left;
        txt.characterSpacing = 6;
        var tr = txt.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(112, 0);
        tr.offsetMax = new Vector2(-70, 0);

        var arrow = NewText("Arrow", go.transform, "»", primary ? 48 : 38);
        arrow.color = accent;
        arrow.fontStyle = FontStyles.Bold;
        Place(arrow.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(40, 50));

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.transition = Selectable.Transition.ColorTint;
        var c = btn.colors;
        if (primary)
        {
            c.normalColor      = new Color(0.08f, 0.45f, 0.95f, 0.85f);
            c.highlightedColor = new Color(0.2f, 0.7f, 1f, 1f);
            c.pressedColor     = new Color(0.05f, 0.3f, 0.7f, 1f);
            c.selectedColor    = c.normalColor;
        }
        else
        {
            c.normalColor      = Glass;
            c.highlightedColor = new Color(0.1f, 0.45f, 0.75f, 0.85f);
            c.pressedColor     = new Color(0.2f, 0.8f, 1f, 0.95f);
            c.selectedColor    = Glass;
        }
        c.fadeDuration = 0.1f;
        btn.colors = c;
        return btn;
    }

    // ---------- Pembuat sprite ----------
    delegate bool InsideFn(float x, float y, float w, float h);

    void CreateSprites()
    {
        Directory.CreateDirectory(Application.dataPath + "/MenuSprites");

        // Jajar genjang (3-slice horizontal)
        slantSprite = Shape("menu_slant", 256, 64, new Vector4(40, 0, 40, 0), (x, y, w, h) => Slant(x, y, w, h, 20f, 0f));
        slantOutline = Shape("menu_slant_outline", 256, 64, new Vector4(40, 0, 40, 0), (x, y, w, h) =>
            Slant(x, y, w, h, 20f, 0f) && !Slant(x, y, w, h, 20f, 3.2f, 3f));

        // Kotak sudut terpotong (9-slice)
        cutSprite = Shape("menu_cut", 128, 64, new Vector4(28, 28, 28, 28), (x, y, w, h) => Cut(x, y, w, h, 24f, 0f));
        cutOutline = Shape("menu_cut_outline", 128, 64, new Vector4(28, 28, 28, 28), (x, y, w, h) =>
            Cut(x, y, w, h, 24f, 0f) && !Cut(x, y, w, h, 24f, 3f));

        // Gradasi alpha kiri -> kanan
        gradientSprite = Gradient("menu_gradient");
    }

    static bool Slant(float x, float y, float w, float h, float s, float inset, float vInset = 0f)
    {
        float k = y / h;
        if (y < vInset || y > h - vInset) return false;
        return x >= s * k + inset && x <= w - s * (1f - k) - inset;
    }

    static bool Cut(float x, float y, float w, float h, float c, float inset)
    {
        if (x < inset || y < inset || x > w - inset || y > h - inset) return false;
        float d = c + inset * 1.4f;
        return (x + (h - y) >= d) && ((w - x) + y >= d);
    }

    Sprite Shape(string name, int w, int h, Vector4 border, InsideFn inside)
    {
        const int SS = 3;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                    for (int sx = 0; sx < SS; sx++)
                        if (inside(x + (sx + 0.5f) / SS, y + (sy + 0.5f) / SS, w, h)) hit++;
                tex.SetPixel(x, y, new Color(1, 1, 1, hit / (float)(SS * SS)));
            }
        return Save(tex, name, border);
    }

    Sprite Gradient(string name)
    {
        int w = 256, h = 4;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = 1f - x / (float)(w - 1);
                tex.SetPixel(x, y, new Color(0, 0.02f, 0.06f, 0.85f * t * t));
            }
        return Save(tex, name, Vector4.zero);
    }

    Sprite Save(Texture2D tex, string name, Vector4 border)
    {
        string path = $"Assets/MenuSprites/{name}.png";
        File.WriteAllBytes(Application.dataPath + $"/MenuSprites/{name}.png", tex.EncodeToPNG());
        DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.spriteBorder = border;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------- Helper ----------
    void Place(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        r.anchorMin = r.anchorMax = anchor;
        r.pivot = pivot;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    Image NewImage(string name, Transform parent, Sprite s, Color c)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = s;
        img.color = (s != null && background != null && s == background) ? Color.white : c;
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
        var rrt = root.GetComponent<RectTransform>();
        rrt.anchorMin = rrt.anchorMax = MidV;
        rrt.sizeDelta = new Vector2(600, 40);

        var back = NewImage("Background", root.transform, null, new Color(0.2f, 0.3f, 0.4f, 0.5f));
        back.raycastTarget = false;
        var brt = back.rectTransform;
        brt.anchorMin = new Vector2(0, 0.35f);
        brt.anchorMax = new Vector2(1, 0.65f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        var fart = fillArea.GetComponent<RectTransform>();
        fart.anchorMin = new Vector2(0, 0.35f);
        fart.anchorMax = new Vector2(1, 0.65f);
        fart.offsetMin = fart.offsetMax = Vector2.zero;
        var fill = NewImage("Fill", fillArea.transform, null, Cyan);
        fill.raycastTarget = false;
        Stretch(fill.rectTransform);

        var handleArea = new GameObject("Handle Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        var hart = handleArea.GetComponent<RectTransform>();
        Stretch(hart);
        hart.offsetMin = new Vector2(10, 0);
        hart.offsetMax = new Vector2(-10, 0);
        var handle = NewImage("Handle", handleArea.transform, slantSprite, Color.white);
        handle.rectTransform.sizeDelta = new Vector2(26, 40);

        var s = root.GetComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.minValue = 0;
        s.maxValue = 1;
        s.value = 1;
        return root;
    }

    void AddAnimation(GameObject btnObj, float delay)
    {
        var anim = btnObj.AddComponent<MenuAnimator>();
        anim.delay = delay;
    }
}
