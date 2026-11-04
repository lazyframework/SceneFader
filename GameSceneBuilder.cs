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

    static readonly Color Blue = new Color(0.16f, 0.42f, 1f);
    static readonly Color Navy = new Color(0.04f, 0.06f, 0.14f, 0.85f);
    static readonly Color Red  = new Color(0.9f, 0.15f, 0.2f);
    static readonly Color Cyan = new Color(0.2f, 0.75f, 1f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.2f);

    Sprite logo, avatar;
    Sprite[] icons = new Sprite[iconNames.Length];
    RenderTexture minimapTexture;

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

    Sprite circleSprite, roundSprite;

    [MenuItem("Tools/Bangun GameScene HUD")]
    static void Open() => GetWindow<GameSceneBuilder>("GameScene HUD Builder");

    void OnGUI()
    {
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

        GUILayout.Space(10);
        if (GUILayout.Button("BANGUN HUD", GUILayout.Height(35))) Build();
    }

    void Build()
    {
        if (GameObject.Find("HUD") != null)
        {
            EditorUtility.DisplayDialog("Sudah ada",
                "HUD sudah ada di scene ini. Hapus objek HUD dan HUDManager lama dulu.", "OK");
            return;
        }

        circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        roundSprite  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

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

        var avatarRing = Anchored("AvatarRing", card, TL, TL, Vector2.zero, new Vector2(160, 160));
        Img(avatarRing, circleSprite, Blue);
        var avatarMask = Full("AvatarMask", avatarRing, 6);
        Img(avatarMask, circleSprite, Navy);
        avatarMask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        var avatarImg = Img(Full("Avatar", avatarMask, 0), avatar, Color.white);
        avatarImg.enabled = avatar != null;

        var lv = Anchored("LevelBadge", card, TL, TL, new Vector2(110, -2), new Vector2(90, 32));
        Img(lv, roundSprite, Navy, Image.Type.Sliced);
        Fill(lv, playerLevel, 22, TextAlignmentOptions.Center);

        var hpFill = Bar("HPBar", card, new Vector2(165, -40), new Vector2(340, 34), Red,
                         out TMP_Text hpText, $"HP   {maxHP} / {maxHP}");
        var mpFill = Bar("MPBar", card, new Vector2(165, -80), new Vector2(340, 34), Cyan,
                         out TMP_Text mpText, $"MP   {maxMP} / {maxMP}");

        // ---------- Kiri atas: quest ----------
        var quest = Anchored("QuestPanel", root, TL, TL, new Vector2(30, -215), new Vector2(560, 150));
        Img(quest, roundSprite, new Color(0.04f, 0.06f, 0.14f, 0.6f), Image.Type.Sliced);

        var questIcon = Anchored("QuestIcon", quest, TL, MID, new Vector2(48, -75), new Vector2(46, 46));
        questIcon.localRotation = Quaternion.Euler(0, 0, 45);
        Img(questIcon, roundSprite, Blue, Image.Type.Sliced);
        Fill(questIcon, "!", 28, TextAlignmentOptions.Center).rectTransform.localRotation =
            Quaternion.Euler(0, 0, -45);

        TextAt("QuestTitle", quest, new Vector2(100, -18), new Vector2(440, 40), questTitle, 28, Gold, TextAlignmentOptions.Left);
        TextAt("QuestDesc",  quest, new Vector2(100, -62), new Vector2(440, 36), questDesc, 24, Color.white, TextAlignmentOptions.Left);
        TextAt("QuestSub",   quest, new Vector2(100, -100), new Vector2(440, 34), questSub, 22, Color.white, TextAlignmentOptions.Left);

        // ---------- Tengah atas: logo ----------
        var logoRT = Anchored("Logo", root, TC, TC, new Vector2(0, -20), new Vector2(420, 130));
        if (logo != null)
            Img(logoRT, logo, Color.white).preserveAspect = true;
        else
            Fill(logoRT, logoText, 56, TextAlignmentOptions.Center);

        // ---------- Kanan atas: Reward, Shop, Bag, Menu ----------
        var topBar = Anchored("TopButtons", root, TR, TR, new Vector2(-30, -30), new Vector2(4 * 120 + 3 * 34, 100));
        var hlg = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 34;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var btnReward = TopButton(topBar, 0);
        var btnShop   = TopButton(topBar, 1);
        var btnBag    = TopButton(topBar, 2);
        var btnMenu   = TopButton(topBar, 3);

        // ---------- Kanan atas: minimap ----------
        var mini = Anchored("Minimap", root, TR, TR, new Vector2(-40, -150), new Vector2(230, 230));
        Img(mini, circleSprite, Blue);
        var miniMask = Full("MinimapMask", mini, 8);
        Img(miniMask, circleSprite, Navy);
        miniMask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        var raw = Full("MinimapView", miniMask, 0).gameObject.AddComponent<RawImage>();
        raw.texture = minimapTexture;
        raw.color = minimapTexture != null ? Color.white : Navy;
        raw.raycastTarget = false;

        var north = Anchored("North", mini, TC, new Vector2(0.5f, 0f), new Vector2(0, 6), new Vector2(40, 30));
        AddText(north, "N", 22, TextAlignmentOptions.Center, Color.white);

        // Nama lokasi di bawah minimap
        var loc = Anchored("LocationBanner", root, TR, TC, new Vector2(-155, -388), new Vector2(260, 44));
        Img(loc, roundSprite, Navy, Image.Type.Sliced);
        Fill(loc, locationName, 22, TextAlignmentOptions.Center);

        // ---------- Kanan bawah: tombol aksi ----------
        var btnRun    = ActionButton(root, 4, new Vector2(-89, 368),  126);
        var btnSkill1 = ActionButton(root, 5, new Vector2(-244, 288), 126);
        var btnSkill2 = ActionButton(root, 6, new Vector2(-359, 162), 138);
        var btnAttack = ActionButton(root, 7, new Vector2(-135, 162), 172);
        var btnJump   = ActionButton(root, 8, new Vector2(-250, 64),   86);

        // ---------- Kiri bawah: area joystick (tempat Joystick Pack) ----------
        var joyRing = Anchored("JoystickArea", root, BL, MID, new Vector2(212, 248), new Vector2(260, 260));
        Img(joyRing, circleSprite, new Color(1, 1, 1, 0.2f));
        Img(Full("JoystickKnob", joyRing, 80), circleSprite, new Color(1, 1, 1, 0.35f));

        // ---------- Kiri bawah: status ----------
        var status = Anchored("StatusBar", root, BL, BL, new Vector2(20, 18), new Vector2(300, 40));
        Img(status, roundSprite, Navy, Image.Type.Sliced);
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

    Image Bar(string name, Transform parent, Vector2 pos, Vector2 size, Color fillColor,
              out TMP_Text label, string text)
    {
        var rt = Anchored(name, parent, TL, TL, pos, size);
        Img(rt, roundSprite, Navy, Image.Type.Sliced);

        var fill = Img(Full("Fill", rt, 3), roundSprite, fillColor, Image.Type.Filled);
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;

        label = Fill(rt, text, 22, TextAlignmentOptions.Center);
        return fill;
    }

    Button TopButton(Transform parent, int index)
    {
        var slot = Anchored(iconNames[index], parent, TL, TL, Vector2.zero, new Vector2(120, 100));
        var slotLE = slot.gameObject.AddComponent<LayoutElement>();
        slotLE.preferredWidth = 120;
        slotLE.preferredHeight = 100;
        var slotImg = Img(slot, null, new Color(0, 0, 0, 0), Image.Type.Simple, true);

        var vlg = slot.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        var iconRT = Anchored("Icon", slot, TL, TL, Vector2.zero, new Vector2(64, 64));
        var iconLE = iconRT.gameObject.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 64;
        iconLE.preferredHeight = 64;
        Sprite icon = icons[index] != null ? icons[index] : circleSprite;
        Img(iconRT, icon, Color.white);

        var labelRT = Anchored("Label", slot, TL, TL, Vector2.zero, new Vector2(120, 28));
        labelRT.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
        AddText(labelRT, iconNames[index], 22, TextAlignmentOptions.Center, Color.white);

        var btn = slot.gameObject.AddComponent<Button>();
        btn.targetGraphic = slotImg;
        btn.transition = Selectable.Transition.None;
        return btn;
    }

    Button ActionButton(Transform parent, int index, Vector2 pos, float size)
    {
        var ring = Anchored(iconNames[index] + " Button", parent, BR, MID, pos, new Vector2(size, size));
        Img(ring, circleSprite, Blue);

        var face = Full("Face", ring, 4);
        var faceImg = Img(face, circleSprite, Navy, Image.Type.Simple, true);

        if (icons[index] != null)
            Img(Full("Icon", face, size * 0.22f), icons[index], Color.white);
        else
            AddText(Full("Label", face, 0), iconNames[index], size > 100 ? 22 : 18,
                    TextAlignmentOptions.Center, Color.white);

        var btn = face.gameObject.AddComponent<Button>();
        btn.targetGraphic = faceImg;
        btn.transition = Selectable.Transition.None;
        return btn;
    }
}
