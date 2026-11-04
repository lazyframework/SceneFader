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
    static readonly string[] iconNames = { "Reward", "Shop", "Bag", "Menu", "Lari", "Skill 1", "Skill 2", "Serang", "Lompat" };

    static readonly Vector2 TL = new Vector2(0f, 1f);
    static readonly Vector2 TR = new Vector2(1f, 1f);
    static readonly Vector2 TC = new Vector2(0.5f, 1f);
    static readonly Vector2 BL = new Vector2(0f, 0f);
    static readonly Vector2 BR = new Vector2(1f, 0f);
    static readonly Vector2 MID = new Vector2(0.5f, 0.5f);

    static readonly Color Navy = new Color(0.04f, 0.06f, 0.14f, 0.85f);
    static readonly Color Red = new Color(1f, 0.1f, 0.1f);  
    static readonly Color Cyan = new Color(0.1f, 0.6f, 1f); 
    static readonly Color Gold = new Color(1f, 0.82f, 0.2f);
    static readonly Color DarkTransparent = new Color(0.05f, 0.05f, 0.05f, 0.7f);
    static readonly Color MapBorderBlue = new Color(0.2f, 0.6f, 1f, 1f);

    Sprite logo, avatar;
    Sprite[] icons = new Sprite[iconNames.Length];
    RenderTexture minimapTexture;
    Sprite circleSprite, roundSprite;

    [MenuItem("Tools/Bangun GameScene HUD")]
    static void Open() => GetWindow<GameSceneBuilder>("GameScene HUD Builder");

    void OnGUI()
    {
        if (GUILayout.Button("BANGUN HUD", GUILayout.Height(40)))
        {
            Build();
        }

        GUILayout.Space(10);
        GUILayout.Label("Aset (boleh dikosongkan)", EditorStyles.boldLabel);
        logo = (Sprite)EditorGUILayout.ObjectField("Logo", logo, typeof(Sprite), false);
        avatar = (Sprite)EditorGUILayout.ObjectField("Avatar", avatar, typeof(Sprite), false);
        minimapTexture = (RenderTexture)EditorGUILayout.ObjectField("Minimap Texture", minimapTexture, typeof(RenderTexture), false);

        GUILayout.Space(6);
        GUILayout.Label("Ikon Tombol", EditorStyles.boldLabel);
        for (int i = 0; i < icons.Length; i++)
            icons[i] = (Sprite)EditorGUILayout.ObjectField(iconNames[i], icons[i], typeof(Sprite), false);
    }

    void Build()
    {
        if (GameObject.Find("HUD") != null)
        {
            EditorUtility.DisplayDialog("Sudah ada", "HUD sudah ada di scene ini. Hapus objek HUD dan HUDManager lama dulu.", "OK");
            return;
        }

        circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var canvasGO = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGO.transform;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var hud = new GameObject("HUDManager").AddComponent<GameHUD>();

        // --- KIRI ATAS: KARTU PEMAIN (MENYATU TANPA BACKGROUND) ---
        var card = Anchored("PlayerCard", root, TL, TL, new Vector2(30, -30), new Vector2(550, 160));

        // 1. AVATAR (Tanpa Background/Border)
        var avatarObj = Anchored("Avatar", card, TL, MID, new Vector2(75, 0), new Vector2(140, 140));
        var avatarImg = avatarObj.gameObject.AddComponent<Image>();
        avatarImg.sprite = avatar;
        // Jika avatar kosong di Builder, beri warna abu transparan agar tetap terlihat
        avatarImg.color = avatar != null ? Color.white : new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        // Masking bulat untuk Avatar
        var mask = avatarObj.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        // 2. LEVEL (Nempel di kanan atas Avatar)
        var lv = Anchored("LevelBadge", card, TL, MID, new Vector2(130, 50), new Vector2(70, 26));
        Img(lv, roundSprite, Navy, Image.Type.Sliced);
        Fill(lv, "Lv.1", 20, TextAlignmentOptions.Center);

        // 3. BAR HP/MP (Menempel langsung di sebelah kanan Avatar)
        var hpFill = Bar("HPBar", card, new Vector2(145, 15), new Vector2(370, 30), Red, out TMP_Text hpText, "HP 100/100");
        var mpFill = Bar("MPBar", card, new Vector2(145, -20), new Vector2(370, 30), Cyan, out TMP_Text mpText, "MP 100/100");


        // --- KIRI ATAS: QUEST ---
        var quest = Anchored("QuestPanel", root, TL, TL, new Vector2(30, -200), new Vector2(560, 150));
        Img(quest, roundSprite, new Color(0.04f, 0.06f, 0.14f, 0.6f), Image.Type.Sliced);
        TextAt("QuestTitle", quest, new Vector2(100, -18), new Vector2(440, 40), "Main Quest", 28, Gold, TextAlignmentOptions.Left);
        TextAt("QuestDesc", quest, new Vector2(100, -62), new Vector2(440, 36), "Go to the City Center", 24, Color.white, TextAlignmentOptions.Left);

        // --- TENGAH ATAS: LOGO ---
        var logoRT = Anchored("Logo", root, TC, TC, new Vector2(0, -20), new Vector2(420, 130));
        if (logo != null) Img(logoRT, logo, Color.white).preserveAspect = true;
        else Fill(logoRT, "Eternal World", 56, TextAlignmentOptions.Center);

        // --- KANAN ATAS: MENU ---
        var topBar = Anchored("TopButtons", root, TR, TR, new Vector2(-30, -30), new Vector2(600, 100));
        var hlg = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 34;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        var btnReward = TopButton(topBar, 0);
        var btnShop = TopButton(topBar, 1);
        var btnBag = TopButton(topBar, 2);
        var btnMenu = TopButton(topBar, 3);

        // --- KANAN ATAS: MINIMAP (BORDER BIRU) ---
        var mini = Anchored("Minimap", root, TR, TR, new Vector2(-40, -150), new Vector2(230, 230));
        var border = Full("Border", mini, -4); 
        var borderImg = border.gameObject.AddComponent<Image>();
        borderImg.sprite = circleSprite;
        borderImg.color = MapBorderBlue;

        var inner = Full("Inner", mini, 0);
        var innerImg = inner.gameObject.AddComponent<Image>();
        innerImg.sprite = circleSprite;
        innerImg.color = new Color(0, 0, 0, 0.5f); 

        var miniMask = Full("MinimapMask", inner, 0);
        var maskImg = miniMask.gameObject.AddComponent<Image>();
        maskImg.sprite = circleSprite;
        miniMask.gameObject.AddComponent<Mask>().showMaskGraphic = false; 

        var raw = Full("MinimapView", miniMask, 0).gameObject.AddComponent<RawImage>();
        raw.texture = minimapTexture;
        raw.color = minimapTexture != null ? Color.white : new Color(0.1f, 0.1f, 0.15f, 0.8f);

        // --- KANAN BAWAH: TOMBOL AKSI ---
        var btnRun = ActionButton(root, 4, new Vector2(-89, 368), 126);
        var btnSkill1 = ActionButton(root, 5, new Vector2(-244, 288), 126);
        var btnSkill2 = ActionButton(root, 6, new Vector2(-359, 162), 138);
        var btnAttack = ActionButton(root, 7, new Vector2(-135, 162), 172);
        var btnJump = ActionButton(root, 8, new Vector2(-250, 64), 86);

        // --- KIRI BAWAH: JOYSTICK ---
        var joyRing = Anchored("JoystickArea", root, BL, MID, new Vector2(212, 248), new Vector2(260, 260));
        Img(joyRing, circleSprite, new Color(1, 1, 1, 0.1f)); 
        Img(Full("JoystickKnob", joyRing, 80), circleSprite, new Color(1, 1, 1, 0.25f));

        // Sambungkan Event Tombol
        UnityEventTools.AddPersistentListener(btnReward.onClick, hud.OnReward);
        UnityEventTools.AddPersistentListener(btnShop.onClick, hud.OpenShop);
        UnityEventTools.AddPersistentListener(btnBag.onClick, hud.OnBag);
        UnityEventTools.AddPersistentListener(btnMenu.onClick, hud.OpenMenu);
        UnityEventTools.AddPersistentListener(btnRun.onClick, hud.OnRun);
        UnityEventTools.AddPersistentListener(btnSkill1.onClick, hud.OnSkill1);
        UnityEventTools.AddPersistentListener(btnSkill2.onClick, hud.OnSkill2);
        UnityEventTools.AddPersistentListener(btnAttack.onClick, hud.OnAttack);
        UnityEventTools.AddPersistentListener(btnJump.onClick, hud.OnJump);

        var hs = new SerializedObject(hud);
        hs.FindProperty("hpFill").objectReferenceValue = hpFill;
        hs.FindProperty("mpFill").objectReferenceValue = mpFill;
        hs.FindProperty("hpText").objectReferenceValue = hpText;
        hs.FindProperty("mpText").objectReferenceValue = mpText;
        hs.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // --- FUNGSI BAR POLOS DENGAN CLIP PATH ---
    Image Bar(string name, Transform parent, Vector2 pos, Vector2 size, Color fillColor, out TMP_Text label, string text)
    {
        var rt = Anchored(name, parent, TL, MID, pos, size);

        var bg = Full("BG", rt, 0).gameObject.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f); 
        
        var maskArea = Full("MaskArea", rt, 2); 
        var maskImg = maskArea.gameObject.AddComponent<Image>();
        maskImg.color = Color.white; 
        maskArea.gameObject.AddComponent<Mask>().showMaskGraphic = false; 

        var fill = Full("Fill", maskArea, 0).gameObject.AddComponent<Image>();
        fill.color = fillColor; 
        fill.type = Image.Type.Filled; 
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;

        var frame = Img(Full("Frame", rt, 0), roundSprite, new Color(1,1,1, 0.2f), Image.Type.Sliced);
        frame.raycastTarget = false;

        label = Fill(rt, text, 20, TextAlignmentOptions.Center);
        label.raycastTarget = false;

        return fill;
    }

    // --- TOMBOL MENU (KANAN ATAS) HITAM TRANSPARAN ---
    Button TopButton(Transform parent, int index)
    {
        var slot = Anchored(iconNames[index], parent, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(80, 100));
        var slotImg = Img(slot, roundSprite, DarkTransparent, Image.Type.Sliced);

        var iconRT = Anchored("Icon", slot, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(60, 60));
        Sprite icon = icons[index] != null ? icons[index] : circleSprite;
        Img(iconRT, icon, Color.white);

        var labelRT = Anchored("Label", slot, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(80, 30));
        AddText(labelRT, iconNames[index], 18, TextAlignmentOptions.Center, Color.white);

        var btn = slot.gameObject.AddComponent<Button>();
        btn.targetGraphic = slotImg;
        btn.transition = Selectable.Transition.ColorTint; 
        return btn;
    }

    // --- TOMBOL AKSI (KANAN BAWAH) HITAM TRANSPARAN ---
    Button ActionButton(Transform parent, int index, Vector2 pos, float size)
    {
        var ring = Anchored(iconNames[index] + " Button", parent, new Vector2(1, 0), new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
        
        var face = Full("Face", ring, 0);
        var faceImg = Img(face, circleSprite, DarkTransparent);

        if (icons[index] != null) 
            Img(Full("Icon", face, size * 0.22f), icons[index], Color.white);
        else 
            AddText(Full("Label", face, 0), iconNames[index], size > 100 ? 22 : 18, TextAlignmentOptions.Center, Color.white);

        var btn = face.gameObject.AddComponent<Button>();
        btn.targetGraphic = faceImg;
        btn.transition = Selectable.Transition.ColorTint;
        return btn;
    }

    // --- HELPER ---
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

    Image Img(RectTransform rt, Sprite s, Color c, Image.Type type = Image.Type.Simple)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = s;
        img.color = c;
        img.type = s != null ? type : Image.Type.Simple;
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

    void TextAt(string name, Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, Color color, TextAlignmentOptions align)
    {
        AddText(Anchored(name, parent, new Vector2(0, 1), new Vector2(0, 1), pos, size), text, fontSize, align, color);
    }
}
