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
    string gameScene  = "GameScene";
    string version    = "v1.0.0";
    string gameTitle  = "ETERNAL WORLD";
    string playerName = "PLAYER";
    Vector2 margin    = new Vector2(50, 40);
    Vector2 scrollPos;

    // --- PALET WARNA CYBER ---
    static readonly Color BrightBlue = new Color(0.1f, 0.45f, 1f);
    static readonly Color NeonBlue   = new Color(0.2f, 0.85f, 1f);
    static readonly Color PanelDark  = new Color(0.02f, 0.08f, 0.22f, 0.85f);
    static readonly Color BarDark    = new Color(0.02f, 0.06f, 0.18f, 0.90f);

    Sprite slantSprite, slantOutline;

    [MenuItem("Tools/Bangun Main Menu")]
    static void Open() => GetWindow<MainMenuBuilder>("Lobby Builder");

    void OnGUI()
    {
        GUILayout.Space(6);
        if (GUILayout.Button("BANGUN LOBBY", GUILayout.Height(40))) Build();
        GUILayout.Space(6);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Aset", EditorStyles.boldLabel);
        background = (Sprite)EditorGUILayout.ObjectField("Background (Latar)", background, typeof(Sprite), false);
        character  = (Sprite)EditorGUILayout.ObjectField("Karakter (Opsional)", character, typeof(Sprite), false);
        customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font TMP (Opsional)", customFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(6);
        GUILayout.Label("Pengaturan", EditorStyles.boldLabel);
        gameScene  = EditorGUILayout.TextField("Nama Scene Game", gameScene);
        gameTitle  = EditorGUILayout.TextField("Judul Game", gameTitle);
        playerName = EditorGUILayout.TextField("Nama Player", playerName);
        version    = EditorGUILayout.TextField("Teks versi", version);
        margin     = EditorGUILayout.Vector2Field("Jarak tepi (kiri/kanan)", margin);
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

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGO.transform;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<StandaloneInputModule>();
        }

        // Background & Karakter
        var bg = NewImage("Background", root, background, Color.white);
        bg.raycastTarget = false;
        Place(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 100));
        bg.gameObject.AddComponent<AutoFitBackground>();
        bg.gameObject.AddComponent<BackgroundAnimator>();

        if (character != null)
        {
            var chr = NewImage("Karakter", root, character, Color.white);
            chr.preserveAspect = true;
            chr.raycastTarget = false;
            var cr = chr.rectTransform;
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0f);
            cr.anchoredPosition = new Vector2(0, 110);
            cr.sizeDelta = new Vector2(1000, 970);
        }

        // Manager
        var menuGO = new GameObject("MenuManager");
        var menu = menuGO.AddComponent<MainMenu>();
        var lobbyUI = menuGO.AddComponent<LobbyUI>();

        BuildTopLeftProfile(root);
        BuildTitle(root);

        // --- KANAN ATAS: MAIL & EVENTS ---
        var TR = new Vector2(1f, 1f);
        var btnMail = MakeSmallTopButton(root, "BtnMail", "MAIL", new Vector2(-margin.x - 60, -30));
        var btnEvents = MakeSmallTopButton(root, "BtnEvents", "EVENTS", new Vector2(-margin.x - 60, -100));

        // --- BAR BAWAH & TOMBOL NAVIGASI UTAMA ---
        var bar = NewImage("BottomBar", root, null, BarDark);
        bar.raycastTarget = false;
        var br = bar.rectTransform;
        br.anchorMin = new Vector2(0, 0);
        br.anchorMax = new Vector2(1, 0);
        br.pivot = new Vector2(0.5f, 0);
        br.anchoredPosition = Vector2.zero;
        br.sizeDelta = new Vector2(0, 130);

        var topLine = NewImage("BarLine", bar.transform, null, NeonBlue);
        topLine.raycastTarget = false;
        Place(topLine.rectTransform, new Vector2(0, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        topLine.rectTransform.anchorMax = new Vector2(1, 1);
        topLine.rectTransform.sizeDelta = new Vector2(0, 3);

        // Tombol Bawah Kiri (Shop, Settings, Quit)
        var BL = new Vector2(0f, 0f);
        Vector2 navSize = new Vector2(220, 65);
        float navGap = 15f;
        float navY = margin.y + 15;

        var shop = MakeButton(root, "BtnShop", "SHOP", BL, BL, new Vector2(margin.x, navY), navSize, false);
        var settings = MakeButton(root, "BtnSettings", "SETTINGS", BL, BL, new Vector2(margin.x + (navSize.x + navGap), navY), navSize, false);
        var quit = MakeButton(root, "BtnQuit", "QUIT", BL, BL, new Vector2(margin.x + (navSize.x + navGap) * 2, navY), navSize, false);

        // Tombol Bawah Tengah (CHARACTER, INVENTORY)
        Vector2 midSize = new Vector2(240, 75);
        var btnChar = MakeButton(root, "BtnCharacter", "CHARACTER", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-130, navY - 5), midSize, false);
        var btnInv = MakeButton(root, "BtnInventory", "INVENTORY", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(130, navY - 5), midSize, false);

        // Tombol START (Kanan Bawah)
        var BR = new Vector2(1f, 0f);
        Vector2 startSize = new Vector2(400, 100);
        var start = MakeButton(root, "BtnStart", "START", BR, BR, new Vector2(-margin.x, navY - 10), startSize, true);

        // --- PANEL POP-UP ---
        var shopPanel = MakePanel(root, "ShopPanel", "SHOP");
        var settingsPanel = MakePanel(root, "SettingsPanel", "SETTINGS");
        
        // Setup Slider di Settings
        var setWin = settingsPanel.transform.Find("Window");
        var volLabel = NewText("VolumeLabel", setWin, "VOLUME", 38);
        volLabel.alignment = TextAlignmentOptions.Left;
        Place(volLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300, 30), new Vector2(300, 60));
        var slider = MakeSlider(setWin, new Vector2(80, 30), new Vector2(420, 40));
        slider.value = 1f;
        UnityEventTools.AddPersistentListener(slider.onValueChanged, lobbyUI.SetVolume);

        // Hubungkan Tombol
        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.QuitGame);
        UnityEventTools.AddBoolPersistentListener(shop.onClick, shopPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(settings.onClick, settingsPanel.SetActive, true);

        // Animasi
        AddAnimation(shop.gameObject, 0.1f);
        AddAnimation(settings.gameObject, 0.15f);
        AddAnimation(quit.gameObject, 0.2f);
        AddAnimation(btnChar.gameObject, 0.25f);
        AddAnimation(btnInv.gameObject, 0.3f);
        AddAnimation(btnMail.gameObject, 0.35f);
        AddAnimation(btnEvents.gameObject, 0.4f);
        AddAnimation(start.gameObject, 0.5f);

        shopPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Versi
        var ver = NewText("Version", root, version, 22);
        ver.alignment = TextAlignmentOptions.Right;
        ver.color = new Color(1f, 1f, 1f, 0.7f);
        Place(ver.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -20), new Vector2(300, 34));

        var so = new SerializedObject(menu);
        so.FindProperty("gameSceneName").stringValue = gameScene;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
    }

    // --- TOMBOL KECIL KANAN ATAS (Mail/Events) ---
    Button MakeSmallTopButton(Transform root, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(root, false);
        Place(go.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), pos, new Vector2(70, 70));

        var bg = go.GetComponent<Image>();
        bg.sprite = slantSprite;
        bg.type = Image.Type.Sliced;
        bg.color = PanelDark;

        var border = NewImage("Outline", go.transform, slantOutline, Color.white);
        border.type = Image.Type.Sliced;
        Stretch(border.rectTransform);

        var txt = NewText("Label", go.transform, label, 20);
        txt.fontStyle = FontStyles.Bold;
        Stretch(txt.rectTransform);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        return btn;
    }

    // --- PROFIL KIRI ATAS ---
    void BuildTopLeftProfile(Transform root)
    {
        var box = NewImage("Profile", root, slantSprite, PanelDark);
        box.type = Image.Type.Sliced;
        box.raycastTarget = false;
        Place(box.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -30), new Vector2(460, 110));

        var outline = NewImage("Outline", box.transform, slantOutline, Color.white);
        outline.type = Image.Type.Sliced;
        Stretch(outline.rectTransform);

        var avatar = NewImage("Avatar", box.transform, null, new Color(0.15f, 0.3f, 0.6f, 1f));
        Place(avatar.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(74, 74));

        var name = NewText("Name", box.transform, playerName, 34);
        name.fontStyle = FontStyles.Bold | FontStyles.Italic;
        name.alignment = TextAlignmentOptions.Left;
        Place(name.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 18), new Vector2(300, 44));

        var lv = NewText("Level", box.transform, "Lv. 1", 26);
        lv.alignment = TextAlignmentOptions.Left;
        lv.color = NeonBlue;
        Place(lv.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, -22), new Vector2(300, 34));
    }

    // --- JUDUL TENGAH ATAS ---
    void BuildTitle(Transform root)
    {
        var t = NewText("Title", root, gameTitle, 76);
        t.fontStyle = FontStyles.Bold | FontStyles.Italic;
        t.characterSpacing = 6;
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(20, 90, 255, 255);
        Place(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(900, 110));
    }

    // --- TOMBOL UTAMA ---
    Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, bool isPrimary)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), anchor, pivot, pos, size);

        var bg = go.GetComponent<Image>();
        bg.sprite = slantSprite;
        bg.type = Image.Type.Sliced;
        bg.color = isPrimary ? BrightBlue : PanelDark;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0.1f, 0.3f, 0.5f);
        shadow.effectDistance = new Vector2(0, -5);

        var border = NewImage("Outline", go.transform, slantOutline, Color.white);
        border.type = Image.Type.Sliced;
        Stretch(border.rectTransform);

        var txt = NewText("Label", go.transform, label, isPrimary ? 46 : 26);
        txt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        txt.characterSpacing = 4;
        Stretch(txt.rectTransform);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        return btn;
    }

    // --- PANEL POP-UP ---
    GameObject MakePanel(Transform root, string name, string title)
    {
        var overlay = NewImage(name, root, null, new Color(0f, 0.02f, 0.08f, 0.8f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = true; 

        var win = NewImage("Window", overlay.transform, slantSprite, new Color(0.03f, 0.1f, 0.26f, 0.95f));
        win.type = Image.Type.Sliced;
        Place(win.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 580));

        var outline = NewImage("Outline", win.transform, slantOutline, Color.white);
        outline.type = Image.Type.Sliced;
        Stretch(outline.rectTransform);

        var t = NewText("Title", win.transform, title, 56);
        t.fontStyle = FontStyles.Bold | FontStyles.Italic;
        Place(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(700, 80));

        var close = MakeButton(win.transform, "BtnClose", "CLOSE", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 36), new Vector2(300, 76), false);
        UnityEventTools.AddBoolPersistentListener(close.onClick, overlay.gameObject.SetActive, false);
        return overlay.gameObject;
    }

    Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);

        var back = NewImage("Background", go.transform, null, new Color(1f, 1f, 1f, 0.15f));
        back.rectTransform.anchorMin = new Vector2(0, 0.35f);
        back.rectTransform.anchorMax = new Vector2(1, 0.65f);
        back.rectTransform.offsetMin = back.rectTransform.offsetMax = Vector2.zero;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var far = fillArea.GetComponent<RectTransform>();
        far.anchorMin = new Vector2(0, 0.35f);
        far.anchorMax = new Vector2(1, 0.65f);
        far.offsetMin = far.offsetMax = Vector2.zero;

        var fill = NewImage("Fill", fillArea.transform, null, NeonBlue);
        fill.rectTransform.sizeDelta = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var har = handleArea.GetComponent<RectTransform>();
        har.anchorMin = Vector2.zero;
        har.anchorMax = Vector2.one;
        har.offsetMin = new Vector2(15, 0);
        har.offsetMax = new Vector2(-15, 0);

        var handle = NewImage("Handle", handleArea.transform, null, Color.white);
        handle.rectTransform.sizeDelta = new Vector2(30, 0);
        handle.rectTransform.anchorMin = new Vector2(0, 0);
        handle.rectTransform.anchorMax = new Vector2(0, 1);

        var s = go.GetComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.minValue = 0f;
        s.maxValue = 1f;
        return s;
    }

    // --- PEMBUAT SPRITE ---
    delegate bool InsideFn(float x, float y, float w, float h);

    void CreateSprites()
    {
        Directory.CreateDirectory(Application.dataPath + "/MenuSprites");

        slantSprite = Shape("menu_slant", 256, 64, new Vector4(40, 0, 40, 0),
            (x, y, w, h) => Slant(x, y, w, h, 20f, 0f, 0f));
        slantOutline = Shape("menu_slant_outline", 256, 64, new Vector4(40, 0, 40, 0),
            (x, y, w, h) => Slant(x, y, w, h, 20f, 0f, 0f) && !Slant(x, y, w, h, 20f, 4f, 3.5f));
    }

    static bool Slant(float x, float y, float w, float h, float s, float inset, float vInset)
    {
        if (y < vInset || y > h - vInset) return false;
        float k = y / h;
        return x >= s * k + inset && x <= w - s * (1f - k) - inset;
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

        string path = $"Assets/MenuSprites/{name}.png";
        File.WriteAllBytes(Application.dataPath + $"/MenuSprites/{name}.png", tex.EncodeToPNG());
        DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.spriteBorder = border;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // --- HELPER ---
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
        img.color = (s != null && s == background) ? Color.white : c;
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

    void AddAnimation(GameObject btnObj, float delay)
    {
        var anim = btnObj.AddComponent<MenuAnimator>();
        anim.delay = delay;
    }
}
