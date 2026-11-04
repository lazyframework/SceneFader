using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GameSceneBuilder : EditorWindow
{
    static readonly string[] iconNames =
        { "Reward", "Shop", "Bag", "Menu", "Lari", "Skill 1", "Skill 2", "Serang", "Lompat" };

    static readonly Vector2 TL  = new Vector2(0f, 1f);
    static readonly Vector2 TR  = new Vector2(1f, 1f);
    static readonly Vector2 TC  = new Vector2(0.5f, 1f);
    static readonly Vector2 BL  = new Vector2(0f, 0f);
    static readonly Vector2 BR  = new Vector2(1f, 0f);
    static readonly Vector2 MID = new Vector2(0.5f, 0.5f);

    static readonly Color Blue      = new Color(0.16f, 0.42f, 1f);
    static readonly Color BlueSoft  = new Color(0.3f, 0.6f, 1f, 0.9f);
    static readonly Color Red       = new Color(0.9f, 0.15f, 0.2f);
    static readonly Color Cyan      = new Color(0.2f, 0.75f, 1f);
    static readonly Color Gold      = new Color(1f, 0.82f, 0.2f);
    static readonly Color Glass     = new Color(0f, 0f, 0f, 0.55f);   // hitam agak transparan
    static readonly Color GlassSoft = new Color(0f, 0f, 0f, 0.40f);
    static readonly Color Clear     = new Color(0f, 0f, 0f, 0.15f);   // isi minimap (hampir transparan)

    const string SpriteFolder = "Assets/HUDSprites";

    Sprite logo, avatar;
    Sprite[] icons = new Sprite[iconNames.Length];
    RenderTexture minimapTexture;
    Vector2 scrollPos;

    string logoText     = "Eternal World";
    string playerLevel  = "Lv.1";
    int    maxHP        = 100;
    int    maxMP        = 100;
    string locationName = "Cartoon City";
    string questTitle   = "Main Quest";
    string questDesc    = "Go to the City Center";
    string questSub     = "• Talk to the Guard  (0 / 1)";
    string statusText   = "120ms   14:30";
    string shopScene    = "ShopScene";
    string menuScene    = "MainMenu";

    // Sprite bentuk (dibuat otomatis)
    Sprite circleSprite;   // lingkaran penuh
    Sprite ringSprite;     // lingkaran berongga (border)
    Sprite cutSprite;      // kotak dengan sudut terpotong (9-slice)
    Sprite slantSprite;    // jajar genjang untuk bar HP/MP

    [MenuItem("Tools/Bangun GameScene HUD")]
    static void Open() => GetWindow<GameSceneBuilder>("GameScene HUD Builder");

    void OnGUI()
    {
        GUILayout.Space(6);
        if (GUILayout.Button("BANGUN HUD", GUILayout.Height(40))) Build();
        GUILayout.Space(6);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Aset (boleh dikosongkan)", EditorStyles.boldLabel);
        logo           = (Sprite)EditorGUILayout.ObjectField("Logo", logo, typeof(Sprite), false);
        avatar         = (Sprite)EditorGUILayout.ObjectField("Avatar", avatar, typeof(Sprite), false);
        minimapTexture = (RenderTexture)EditorGUILayout.ObjectField("Render Texture Minimap", minimapTexture, typeof(RenderTexture), false);

        GUILayout.Space(6);
        GUILayout.Label("Ikon (urutan: Reward, Shop, Bag, Menu, Lari, Skill 1, Skill 2, Serang, Lompat)",
                        EditorStyles.wordWrappedLabel);
        for (int i = 0; i < icons.Length; i++)
            icons[i] = (Sprite)EditorGUILayout.ObjectField(iconNames[i], icons[i], typeof(Sprite), false);

        GUILayout.Space(6);
        GUILayout.Label("Teks & data", EditorStyles.boldLabel);
        logoText     = EditorGUILayout.TextField("Teks logo (jika tanpa logo)", logoText);
        playerLevel  = EditorGUILayout.TextField("Level", playerLevel);
        maxHP        = EditorGUILayout.IntField("HP maks", maxHP);
        maxMP        = EditorGUILayout.IntField("MP maks", maxMP);
        locationName = EditorGUILayout.TextField("Nama lokasi", locationName);
        questTitle   = EditorGUILayout.TextField("Judul quest", questTitle);
        questDesc    = EditorGUILayout.TextField("Deskripsi quest", questDesc);
        questSub     = EditorGUILayout.TextField("Sub quest", questSub);
        statusText   = EditorGUILayout.TextField("Teks status", statusText);
        shopScene    = EditorGUILayout.TextField("Nama Scene Shop", shopScene);
        menuScene    = EditorGUILayout.TextField("Nama Scene Menu", menuScene);

        EditorGUILayout.EndScrollView();
    }

    void Build()
    {
        if (GameObject.Find("HUD") != null)
        {
            EditorUtility.DisplayDialog("Sudah ada",
                "HUD sudah ada di scene ini. Hapus objek HUD dan HUDManager lama dulu.", "OK");
            return;
        }

        CreateShapeSprites();

        // Canvas
        var canvasGO = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGO.transform;

        // EventSystem (Input System)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        // Manager HUD
        var hud = new GameObject("HUDManager").AddComponent<GameHUD>();

        // ---------- Kiri atas: kartu pemain ----------
        var card = Anchored("PlayerCard", root, TL, TL, new Vector2(30, -30), new Vector2(520, 170));

        // Avatar: tanpa background, hanya gambar yang dipotong lingkaran + border tipis
        var avatarRoot = Anchored("Avatar", card, TL, TL, Vector2.zero, new Vector2(160, 160));
        var avatarMask = Full("AvatarMask", avatarRoot, 6);
        Img(avatarMask, circleSprite, Color.white);
        avatarMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var avatarImg = Img(Full("AvatarImage", avatarMask, 0), avatar, Color.white);
        avatarImg.enabled = avatar != null;
        Img(Full("AvatarBorder", avatarRoot, 0), ringSprite, Blue);

        var lv = Anchored("LevelBadge", card, TL, TL, new Vector2(110, -2), new Vector2(90, 34));
        Img(lv, cutSprite, Glass, Image.Type.Sliced);
        Fill(lv, playerLevel, 22, TextAlignmentOptions.Center);

        var hpFill = Bar("HPBar", card, new Vector2(150, -42), new Vector2(360, 34), Red,
                         out TMP_Text hpText, $"HP   {maxHP} / {maxHP}");
        var mpFill = Bar("MPBar", card, new Vector2(138, -80), new Vector2(360, 34), Cyan,
                         out TMP_Text mpText, $"MP   {maxMP} / {maxMP}");

        // ---------- Kiri atas: quest ----------
        var quest = Anchored("QuestPanel", root, TL, TL, new Vector2(0, -215), new Vector2(560, 150));
        Img(quest, cutSprite, GlassSoft, Image.Type.Sliced);

        var questIcon = Anchored("QuestIcon", quest, TL, MID, new Vector2(48, -75), new Vector2(46, 46));
        questIcon.localRotation = Quaternion.Euler(0, 0, 45);
        Img(questIcon, cutSprite, Blue, Image.Type.Sliced);
        var excl = Fill(questIcon, "!", 28, TextAlignmentOptions.Center);
        excl.rectTransform.localRotation = Quaternion.Euler(0, 0, -45);

        TextAt("QuestTitle", quest, new Vector2(100, -18),  new Vector2(440, 40), questTitle, 28, Gold,        TextAlignmentOptions.Left);
        TextAt("QuestDesc",  quest, new Vector2(100, -62),  new Vector2(440, 36), questDesc,  24, Color.white, TextAlignmentOptions.Left);
        TextAt("QuestSub",   quest, new Vector2(100, -100), new Vector2(440, 34), questSub,   22, Color.white, TextAlignmentOptions.Left);

        // ---------- Tengah atas: logo ----------
        var logoRT = Anchored("Logo", root, TC, TC, new Vector2(0, -20), new Vector2(420, 130));
        if (logo != null)
            Img(logoRT, logo, Color.white).preserveAspect = true;
        else
            Fill(logoRT, logoText, 56, TextAlignmentOptions.Center);

        // ---------- Kanan atas: Reward, Shop, Bag, Menu ----------
        var btnMenu   = TopButton(root, 3, new Vector2(-40, -20));
        var btnBag    = TopButton(root, 2, new Vector2(-166, -20));
        var btnShop   = TopButton(root, 1, new Vector2(-292, -20));
        var btnReward = TopButton(root, 0, new Vector2(-418, -20));

        // ---------- Kanan atas: minimap transparan + border ----------
        var mini = Anchored("Minimap", root, TR, TR, new Vector2(-40, -150), new Vector2(230, 230));
        var miniMask = Full("MinimapMask", mini, 6);
        Img(miniMask, circleSprite, Color.white);
        miniMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var raw = Full("MinimapView", miniMask, 0).gameObject.AddComponent<RawImage>();
        raw.texture = minimapTexture;
        raw.color = minimapTexture != null ? new Color(1, 1, 1, 0.85f) : Clear;
        raw.raycastTarget = false;
        Img(Full("MinimapBorder", mini, 0), ringSprite, BlueSoft);   // border di atas peta

        var north = Anchored("North", mini, TC, new Vector2(0.5f, 0.5f), new Vector2(0, -2), new Vector2(40, 30));
        AddText(north, "N", 22, TextAlignmentOptions.Center, Color.white);

        // Nama lokasi di bawah minimap
        var loc = Anchored("LocationBanner", root, TR, TC, new Vector2(-155, -388), new Vector2(260, 44));
        Img(loc, cutSprite, Glass, Image.Type.Sliced);
        Fill(loc, locationName, 22, TextAlignmentOptions.Center);

        // ---------- Kanan bawah: tombol aksi (hitam transparan) ----------
        var btnRun    = ActionButton(root, 4, new Vector2(-89, 368),  126);
        var btnSkill1 = ActionButton(root, 5, new Vector2(-244, 288), 126);
        var btnSkill2 = ActionButton(root, 6, new Vector2(-359, 162), 138);
        var btnAttack = ActionButton(root, 7, new Vector2(-135, 162), 172);
        var btnJump   = ActionButton(root, 8, new Vector2(-250, 64),   86);

        // ---------- Kiri bawah: area joystick ----------
        var joyRing = Anchored("JoystickArea", root, BL, MID, new Vector2(212, 248), new Vector2(260, 260));
        Img(joyRing, ringSprite, new Color(1, 1, 1, 0.45f));
        Img(Full("JoystickKnob", joyRing, 80), circleSprite, new Color(1, 1, 1, 0.35f));

        // ---------- Kiri bawah: status ----------
        var status = Anchored("StatusBar", root, BL, BL, new Vector2(20, 18), new Vector2(300, 40));
        Img(status, cutSprite, Glass, Image.Type.Sliced);
        Fill(status, statusText, 20, TextAlignmentOptions.Center);

        // Sambungkan event tombol
        UnityEventTools.AddPersistentListener(btnReward.onClick, hud.OnReward);
        UnityEventTools.AddPersistentListener(btnShop.onClick,   hud.OpenShop);
        UnityEventTools.AddPersistentListener(btnBag.onClick,    hud.OnBag);
        UnityEventTools.AddPersistentListener(btnMenu.onClick,   hud.OpenMenu);
        UnityEventTools.AddPersistentListener(btnRun.onClick,    hud.OnRun);
        UnityEventTools.AddPersistentListener(btnSkill1.onClick, hud.OnSkill1);
        UnityEventTools.AddPersistentListener(btnSkill2.onClick, hud.OnSkill2);
        UnityEventTools.AddPersistentListener(btnAttack.onClick, hud.OnAttack);
        UnityEventTools.AddPersistentListener(btnJump.onClick,   hud.OnJump);

        // Isi field GameHUD
        var hs = new SerializedObject(hud);
        hs.FindProperty("hpFill").objectReferenceValue = hpFill;
        hs.FindProperty("mpFill").objectReferenceValue = mpFill;
        hs.FindProperty("hpText").objectReferenceValue = hpText;
        hs.FindProperty("mpText").objectReferenceValue = mpText;
        hs.FindProperty("shopSceneName").stringValue = shopScene;
        hs.FindProperty("menuSceneName").stringValue = menuScene;
        hs.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
        Debug.Log("HUD GameScene selesai dibuat. Simpan scene (Ctrl+S).");
    }

    // ---------- Pembuat sprite bentuk ----------
    delegate bool InsideFn(float x, float y, float w, float h);

    void CreateShapeSprites()
    {
        Directory.CreateDirectory(Application.dataPath + "/HUDSprites");

        circleSprite = MakeShape("hud_circle", 256, 256, Vector4.zero,
            (x, y, w, h) =>
            {
                float dx = x - w / 2f, dy = y - h / 2f, r = w / 2f;
                return dx * dx + dy * dy <= r * r;
            });

        ringSprite = MakeShape("hud_ring", 256, 256, Vector4.zero,
            (x, y, w, h) =>
            {
                float dx = x - w / 2f, dy = y - h / 2f, r = w / 2f, t = 7f;
                float d2 = dx * dx + dy * dy;
                return d2 <= r * r && d2 >= (r - t) * (r - t);
            });

        // Sudut kiri atas dan kanan bawah terpotong
        cutSprite = MakeShape("hud_cut", 128, 64, new Vector4(24, 24, 24, 24),
            (x, y, w, h) =>
            {
                float c = 20f;
                return (x + (h - y) >= c) && ((w - x) + y >= c);
            });

        // Jajar genjang miring seperti bar HP/MP
        slantSprite = MakeShape("hud_slant", 256, 64, Vector4.zero,
            (x, y, w, h) =>
            {
                float s = 18f, k = y / h;
                return x >= s * k && x <= w - s * (1f - k);
            });
    }

    Sprite MakeShape(string name, int w, int h, Vector4 border, InsideFn inside)
    {
        const int SS = 3; // supersampling untuk tepi halus
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                    for (int sx = 0; sx < SS; sx++)
                        if (inside(x + (sx + 0.5f) / SS, y + (sy + 0.5f) / SS, w, h)) hit++;
                tex.SetPixel(x, y, new Color(1, 1, 1, hit / (float)(SS * SS)));
            }
        }
        tex.Apply();

        string path = $"{SpriteFolder}/{name}.png";
        File.WriteAllBytes(Application.dataPath + $"/HUDSprites/{name}.png", tex.EncodeToPNG());
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

    // ---------- Helper ----------
    RectTransform Anchored(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    RectTransform Full(string name, Transform parent, float pad)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
        return rt;
    }

    Image Img(RectTransform rt, Sprite s, Color c, Image.Type type = Image.Type.Simple, bool raycast = false)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = s;
        img.color = c;
        img.type = s != null ? type : Image.Type.Simple;
        img.raycastTarget = raycast;
        return img;
    }

    TMP_Text AddText(RectTransform rt, string text, float fontSize, TextAlignmentOptions align, Color color)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    TMP_Text Fill(Transform parent, string text, float fontSize, TextAlignmentOptions align)
    {
        return AddText(Full("Label", parent, 0), text, fontSize, align, Color.white);
    }

    void TextAt(string name, Transform parent, Vector2 pos, Vector2 size, string text,
                float fontSize, Color color, TextAlignmentOptions align)
    {
        AddText(Anchored(name, parent, TL, TL, pos, size), text, fontSize, align, color);
    }

    // Bar HP/MP berbentuk jajar genjang
    Image Bar(string name, Transform parent, Vector2 pos, Vector2 size, Color fillColor,
              out TMP_Text label, string text)
    {
        var rt = Anchored(name, parent, TL, TL, pos, size);
        Img(rt, slantSprite, Glass);

        var fill = Img(Full("Fill", rt, 3), slantSprite, fillColor, Image.Type.Filled);
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;

        label = Fill(rt, text, 22, TextAlignmentOptions.Center);
        return fill;
    }

    // Tombol atas kanan: lingkaran hitam transparan + ikon + label
    Button TopButton(Transform parent, int index, Vector2 pos)
    {
        var slot = Anchored(iconNames[index] + " Button", parent, TR, TR, pos, new Vector2(120, 110));
        var slotImg = Img(slot, null, new Color(0, 0, 0, 0), Image.Type.Simple, true);

        var bg = Anchored("Bg", slot, TC, TC, new Vector2(0, -2), new Vector2(72, 72));
        Img(bg, circleSprite, Glass);
        Img(Full("Border", bg, 0), ringSprite, new Color(1, 1, 1, 0.5f));

        var iconRT = Anchored("Icon", slot, TC, TC, new Vector2(0, -12), new Vector2(52, 52));
        Img(iconRT, icons[index] != null ? icons[index] : circleSprite, Color.white);

        var labelRT = Anchored("Label", slot, TC, TC, new Vector2(0, -78), new Vector2(120, 30));
        AddText(labelRT, iconNames[index], 22, TextAlignmentOptions.Center, Color.white);

        var btn = slot.gameObject.AddComponent<Button>();
        btn.targetGraphic = slotImg;
        btn.transition = Selectable.Transition.None;
        return btn;
    }

    // Tombol aksi: wajah hitam transparan + border biru tipis di atasnya
    Button ActionButton(Transform parent, int index, Vector2 pos, float size)
    {
        var root = Anchored(iconNames[index] + " Button", parent, BR, MID, pos, new Vector2(size, size));

        var face = Full("Face", root, 0);
        var faceImg = Img(face, circleSprite, Glass, Image.Type.Simple, true);

        if (icons[index] != null)
            Img(Full("Icon", face, size * 0.2f), icons[index], Color.white);
        else
            AddText(Full("Label", face, 0), iconNames[index], size > 100 ? 22 : 18,
                    TextAlignmentOptions.Center, Color.white);

        Img(Full("Border", face, 0), ringSprite, BlueSoft);

        var btn = face.gameObject.AddComponent<Button>();
        btn.targetGraphic = faceImg;
        btn.transition = Selectable.Transition.ColorTint;
        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor     = new Color(0.6f, 0.6f, 0.6f, 1f);
        cb.selectedColor    = Color.white;
        btn.colors = cb;
        return btn;
    }
}
