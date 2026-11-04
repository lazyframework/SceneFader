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
    string version   = "v1.0.0";
    Vector2 margin   = new Vector2(70, 55);
    Vector2 scrollPos;

    // --- PALET WARNA (SESUAI GAMBAR REFERENSI KEDUA) ---
    static readonly Color BrightBlue = new Color(0.1f, 0.45f, 1f);    // Biru tombol START
    static readonly Color NeonBlue   = new Color(0.2f, 0.8f, 1f);     // Biru aksen / border
    static readonly Color WhiteSolid = new Color(0.95f, 0.98f, 1f);   // Putih kebiruan
    static readonly Color DarkBlue   = new Color(0.05f, 0.15f, 0.35f); // Teks biru gelap

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
        character  = (Sprite)EditorGUILayout.ObjectField("Karakter (Tengah)", character, typeof(Sprite), false);
        customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font TMP (Opsional)", customFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(6);
        GUILayout.Label("Pengaturan", EditorStyles.boldLabel);
        gameScene = EditorGUILayout.TextField("Nama Scene Game", gameScene);
        version   = EditorGUILayout.TextField("Teks versi", version);
        margin    = EditorGUILayout.Vector2Field("Jarak tombol (kanan, bawah)", margin);
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

        // EventSystem (Standalone Input Module)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Background
        var bg = NewImage("Background", root, background, Color.white);
        bg.raycastTarget = false;
        Place(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 100));
        bg.gameObject.AddComponent<AutoFitBackground>();
        bg.gameObject.AddComponent<BackgroundAnimator>();

        // Karakter Utama (Di tengah)
        var chr = NewImage("Karakter", root, character, Color.white);
        chr.preserveAspect = true;
        chr.raycastTarget = false;
        chr.enabled = character != null;
        var cr = chr.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0f);
        cr.anchoredPosition = Vector2.zero;
        cr.sizeDelta = new Vector2(1000, 1080);

        // Manager
        var menu = new GameObject("MenuManager").AddComponent<MainMenu>();

        // --- TOMBOL KANAN BAWAH (QUIT & START) ---
        var BRc = new Vector2(1f, 0f);
        Vector2 startSize = new Vector2(540, 112);
        Vector2 quitSize  = new Vector2(340, 84);
        float gap = 26f;

        // START: Tombol Biru Solid
        var start = MakeButton(root, "BtnStart", "START", BRc,
                               new Vector2(-margin.x, margin.y), startSize, true);
        
        // QUIT: Tombol Putih Solid
        var quit  = MakeButton(root, "BtnQuit", "QUIT", BRc,
                               new Vector2(-(margin.x + startSize.x + gap), margin.y + (startSize.y - quitSize.y) * 0.5f),
                               quitSize, false);

        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(quit.onClick,  menu.QuitGame);

        AddAnimation(quit.gameObject, 0.1f);
        AddAnimation(start.gameObject, 0.2f);

        // Teks Versi (Kiri bawah)
        var ver = NewText("Version", root, version, 22);
        ver.alignment = TextAlignmentOptions.Left;
        ver.color = new Color(1f, 1f, 1f, 0.7f); // Putih transparan
        Place(ver.rectTransform, Vector2.zero, Vector2.zero, new Vector2(40, 30), new Vector2(300, 34));

        var so = new SerializedObject(menu);
        so.FindProperty("gameSceneName").stringValue = gameScene;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
    }

    // --- FUNGSI PEMBUAT TOMBOL (PUTIH & BIRU) ---
    Button MakeButton(Transform parent, string name, string label, Vector2 anchor,
                      Vector2 pos, Vector2 size, bool isPrimary)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), anchor, anchor, pos, size);

        // Latar Belakang (Biru untuk Primary, Putih untuk Sekunder)
        var bg = go.GetComponent<Image>();
        bg.sprite = slantSprite;
        bg.type = Image.Type.Sliced;
        bg.color = isPrimary ? BrightBlue : WhiteSolid;

        // Bayangan tipis
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0.1f, 0.3f, 0.4f);
        shadow.effectDistance = new Vector2(0, -5);

        // Garis Tepi
        var borderColor = isPrimary ? NeonBlue : new Color(0.8f, 0.9f, 1f);
        var border = NewImage("Outline", go.transform, slantOutline, borderColor);
        border.type = Image.Type.Sliced;
        border.raycastTarget = false;
        Stretch(border.rectTransform);

        // Garis Aksen Miring di Kiri
        var bar = NewImage("Accent", go.transform, null, isPrimary ? Color.white : BrightBlue);
        bar.raycastTarget = false;
        Place(bar.rectTransform, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 0),
              new Vector2(isPrimary ? 8 : 6, size.y * 0.5f));
        bar.rectTransform.localRotation = Quaternion.Euler(0, 0, -17);

        // Teks (Putih untuk Primary, Biru untuk Sekunder)
        var txt = NewText("Label", go.transform, label, isPrimary ? 52 : 36);
        txt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        txt.characterSpacing = isPrimary ? 10 : 8;
        txt.color = isPrimary ? Color.white : DarkBlue;
        txt.alignment = TextAlignmentOptions.Center;
        Stretch(txt.rectTransform);

        // Panah Kanan
        var arrow = NewText("Arrow", go.transform, "»", isPrimary ? 56 : 40);
        arrow.color = isPrimary ? Color.white : BrightBlue;
        arrow.fontStyle = FontStyles.Bold;
        Place(arrow.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-48, 2), new Vector2(44, 60));

        // Logika Tombol
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.transition = Selectable.Transition.ColorTint;
        var c = btn.colors;
        
        if (isPrimary)
        {
            c.normalColor      = Color.white;
            c.highlightedColor = new Color(0.6f, 0.8f, 1f); // Sedikit lebih terang
            c.pressedColor     = new Color(0.4f, 0.6f, 0.8f);
            c.selectedColor    = Color.white;
        }
        else
        {
            c.normalColor      = Color.white;
            c.highlightedColor = new Color(0.85f, 0.93f, 1f); // Sedikit kebiruan
            c.pressedColor     = new Color(0.7f, 0.85f, 1f);
            c.selectedColor    = Color.white;
        }
        c.fadeDuration = 0.1f;
        btn.colors = c;

        return btn;
    }

    // --- PEMBUAT SPRITE (Bentuk Jajar Genjang) ---
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
