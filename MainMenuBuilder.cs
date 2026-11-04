using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Taruh di Assets/Editor
public class MainMenuBuilder : EditorWindow
{
    Sprite character;
    TMP_FontAsset customFont;
    string gameScene  = "GameScene";
    string version    = "v1.0.0";
    string playerName = "PLAYER";
    Vector2 margin    = new Vector2(70, 40);
    float startRaise  = 45f; // makin besar = tombol START makin naik
    Vector2 scrollPos;

    // --- PALET WARNA CYBER ---
    static readonly Color BrightBlue = new Color(0.1f, 0.45f, 1f);
    static readonly Color NeonBlue   = new Color(0.2f, 0.85f, 1f);
    static readonly Color PanelDark  = new Color(0.02f, 0.08f, 0.22f, 0.82f);
    static readonly Color BarDark    = new Color(0.02f, 0.06f, 0.18f, 0.80f);
    static readonly Vector2 Ctr      = new Vector2(0.5f, 0.5f);

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
        character  = (Sprite)EditorGUILayout.ObjectField("Karakter (Opsional)", character, typeof(Sprite), false);
        customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font TMP (Opsional)", customFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(6);
        GUILayout.Label("Pengaturan", EditorStyles.boldLabel);
        gameScene  = EditorGUILayout.TextField("Nama Scene Game", gameScene);
        playerName = EditorGUILayout.TextField("Nama Player", playerName);
        version    = EditorGUILayout.TextField("Teks versi", version);
        margin     = EditorGUILayout.Vector2Field("Jarak tepi (kiri/kanan, bawah)", margin);
        startRaise = EditorGUILayout.FloatField("START naik ke atas", startRaise);
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

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<StandaloneInputModule>();
        }

        // Karakter (opsional, di tengah)
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

        // --- KIRI ATAS: PROFIL (bisa diklik) ---
        var profileBtn = BuildTopLeftProfile(root);

        // --- KANAN ATAS: MAIL, EVENTS, GOLD, VERSI ---
        var TR = new Vector2(1f, 1f);
        var mail   = MakeButton(root, "BtnMail", "MAIL", TR, TR, new Vector2(-246, -30), new Vector2(190, 64), false, true, 28);
        var events = MakeButton(root, "BtnEvents", "EVENTS", TR, TR, new Vector2(-40, -30), new Vector2(190, 64), false, true, 28);

        var gold = NewText("Gold", root, "GOLD  0", 30);
        gold.fontStyle = FontStyles.Bold | FontStyles.Italic;
        gold.alignment = TextAlignmentOptions.Right;
        gold.color = NeonBlue;
        Place(gold.rectTransform, TR, TR, new Vector2(-40, -110), new Vector2(400, 40));
        lobbyUI.goldText = gold;

        var ver = NewText("Version", root, version, 22);
        ver.alignment = TextAlignmentOptions.Right;
        ver.color = new Color(1f, 1f, 1f, 0.7f);
        Place(ver.rectTransform, TR, TR, new Vector2(-40, -150), new Vector2(300, 34));

        // --- BAR BAWAH ---
        var bar = NewImage("BottomBar", root, null, BarDark);
        bar.raycastTarget = false;
        var br = bar.rectTransform;
        br.anchorMin = new Vector2(0, 0);
        br.anchorMax = new Vector2(1, 0);
        br.pivot = new Vector2(0.5f, 0);
        br.anchoredPosition = Vector2.zero;
        br.sizeDelta = new Vector2(0, 110);

        var topLine = NewImage("BarLine", bar.transform, null, NeonBlue);
        topLine.raycastTarget = false;
        Place(topLine.rectTransform, new Vector2(0, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        topLine.rectTransform.anchorMax = new Vector2(1, 1);
        topLine.rectTransform.sizeDelta = new Vector2(0, 3);

        // --- TOMBOL BAR BAWAH (kiri): INVENTORY, CHARACTERS (besar), SHOP, SETTINGS, QUIT ---
        var BL = new Vector2(0f, 0f);
        string[] navNames  = { "BtnInventory", "BtnCharacters", "BtnShop", "BtnSettings", "BtnQuit" };
        string[] navLabels = { "INVENTORY", "CHARACTERS", "SHOP", "SETTINGS", "QUIT" };
        float[] navW = { 210, 320, 150, 190, 130 };
        float navGap = 6f;
        float navY = margin.y - 4;
        var nav = new Button[navNames.Length];
        float nx = margin.x;
        for (int i = 0; i < nav.Length; i++)
        {
            bool big = (i == 1);
            nav[i] = MakeButton(root, navNames[i], navLabels[i], BL, BL, new Vector2(nx, navY),
                new Vector2(navW[i], big ? 84 : 76), false, true, big ? 32 : 26);
            nx += navW[i] + navGap;
        }
        nav[1].GetComponent<Image>().color = new Color(0.05f, 0.28f, 0.7f, 0.92f); // CHARACTERS lebih menonjol
        var inventory = nav[0]; var characters = nav[1]; var shop = nav[2]; var settings = nav[3]; var quit = nav[4];

        // --- START (kanan bawah, besar) ---
        var BR = new Vector2(1f, 0f);
        var start = MakeButton(root, "BtnStart", "START", BR, BR,
            new Vector2(-margin.x, margin.y + startRaise), new Vector2(600, 128), true);

        // --- PANEL POP-UP ---
        var profilePanel   = BuildProfilePanel(root);
        var charactersPanel = BuildCharactersPanel(root, lobbyUI);
        var inventoryPanel = BuildInventoryPanel(root);
        var shopPanel      = BuildShopPanel(root);
        var settingsPanel  = BuildSettingsPanel(root, lobbyUI);
        var mailPanel      = BuildMailPanel(root, lobbyUI);
        var eventsPanel    = BuildEventsPanel(root, lobbyUI);

        // Hubungkan tombol
        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.QuitGame);
        UnityEventTools.AddBoolPersistentListener(profileBtn.onClick, profilePanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(characters.onClick, charactersPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(inventory.onClick, inventoryPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(shop.onClick, shopPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(settings.onClick, settingsPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(mail.onClick, mailPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(events.onClick, eventsPanel.SetActive, true);

        AddAnimation(mail.gameObject, 0.1f);
        AddAnimation(events.gameObject, 0.15f);
        for (int i = 0; i < nav.Length; i++) AddAnimation(nav[i].gameObject, 0.1f + i * 0.05f);
        AddAnimation(start.gameObject, 0.4f);

        // Semua panel disembunyikan di awal
        foreach (var p in new[] { profilePanel, charactersPanel, inventoryPanel, shopPanel, settingsPanel, mailPanel, eventsPanel })
            p.SetActive(false);

        EditorUtility.SetDirty(lobbyUI);

        var so = new SerializedObject(menu);
        so.FindProperty("gameSceneName").stringValue = gameScene;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;
    }

    // =========================== PANEL-PANEL ===========================

    GameObject BuildProfilePanel(Transform root)
    {
        var panel = MakePanel(root, "ProfilePanel", "PROFILE");
        var win = panel.transform.Find("Window");

        Label(win, playerName + "   -   Lv. 1", 30, new Vector2(0, 165), new Vector2(700, 44),
              TextAlignmentOptions.Center, NeonBlue, true);

        string[] names  = { "LEVEL", "HP", "MP", "ATK", "DEF", "SPD" };
        string[] values = { "1", "100 / 100", "100 / 100", "10", "5", "5" };
        for (int i = 0; i < names.Length; i++)
        {
            float x = (i % 2 == 0) ? -205 : 205;
            float y = 100 - (i / 2) * 76;
            var row = Box(win, "Stat_" + names[i], new Vector2(x, y), new Vector2(380, 60), PanelDark);
            Label(row.transform, names[i], 28, new Vector2(-90, 0), new Vector2(160, 40),
                  TextAlignmentOptions.Left, Color.white, true);
            Label(row.transform, values[i], 28, new Vector2(90, 0), new Vector2(170, 40),
                  TextAlignmentOptions.Right, NeonBlue, true);
        }
        return panel;
    }

    GameObject BuildCharactersPanel(Transform root, LobbyUI lobbyUI)
    {
        var panel = MakePanel(root, "CharactersPanel", "CHARACTERS");
        var win = panel.transform.Find("Window");

        string[] names = { "ARION", "LUNA", "KAI", "ZERO" };
        Color[] tints = {
            new Color(0.2f, 0.5f, 0.95f), new Color(0.6f, 0.35f, 0.9f),
            new Color(0.15f, 0.7f, 0.65f), new Color(0.85f, 0.4f, 0.5f)
        };
        float w = 190f, gap = 14f;
        float total = w * names.Length + gap * (names.Length - 1);
        var cards = new Image[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            float x = -total * 0.5f + w * 0.5f + i * (w + gap);
            var card = Box(win, "Card_" + names[i], new Vector2(x, 25), new Vector2(w, 260), PanelDark);
            card.raycastTarget = true;

            var b = card.gameObject.AddComponent<Button>();
            b.targetGraphic = card;
            b.transition = Selectable.Transition.ColorTint;
            var c = b.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(0.75f, 0.92f, 1f);
            c.pressedColor = new Color(0.5f, 0.7f, 0.9f);
            c.selectedColor = Color.white;
            c.fadeDuration = 0.1f;
            b.colors = c;

            Box(card.transform, "Portrait", new Vector2(0, 35), new Vector2(150, 140), tints[i]);
            Label(card.transform, names[i], 30, new Vector2(0, -75), new Vector2(170, 40),
                  TextAlignmentOptions.Center, Color.white, true);
            Label(card.transform, "Lv. 1", 22, new Vector2(0, -105), new Vector2(170, 30),
                  TextAlignmentOptions.Center, NeonBlue);

            UnityEventTools.AddIntPersistentListener(b.onClick, lobbyUI.SelectCharacter, i);
            cards[i] = card;
        }
        lobbyUI.characterCards = cards;
        return panel;
    }

    GameObject BuildInventoryPanel(Transform root)
    {
        var panel = MakePanel(root, "InventoryPanel", "INVENTORY");
        var win = panel.transform.Find("Window");

        Label(win, "0 / 18", 26, new Vector2(330, 222), new Vector2(160, 36),
              TextAlignmentOptions.Right, NeonBlue, true);

        int cols = 6, rows = 3;
        float s = 100f, g = 12f;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                float x = -(cols - 1) * (s + g) * 0.5f + c * (s + g);
                float y = 122 - r * (s + g);
                Box(win, "Slot_" + (r * cols + c), new Vector2(x, y), new Vector2(s, s), new Color(0.05f, 0.15f, 0.35f, 0.6f));
            }
        return panel;
    }

    GameObject BuildShopPanel(Transform root)
    {
        var panel = MakePanel(root, "ShopPanel", "SHOP");
        var win = panel.transform.Find("Window");
        Label(win, "Toko segera hadir...", 40, Vector2.zero, new Vector2(700, 80),
              TextAlignmentOptions.Center, new Color(0.8f, 0.92f, 1f));
        return panel;
    }

    GameObject BuildSettingsPanel(Transform root, LobbyUI lobbyUI)
    {
        var panel = MakePanel(root, "SettingsPanel", "SETTINGS");
        var win = panel.transform.Find("Window");

        Label(win, "VOLUME", 38, new Vector2(-300, 140), new Vector2(300, 60),
              TextAlignmentOptions.Left, Color.white, true);
        var slider = MakeSlider(win, new Vector2(80, 140), new Vector2(420, 40));
        slider.value = 1f;
        UnityEventTools.AddPersistentListener(slider.onValueChanged, lobbyUI.SetVolume);

        Label(win, "GRAPHICS", 38, new Vector2(-300, 30), new Vector2(300, 60),
              TextAlignmentOptions.Left, Color.white, true);

        var images = new Image[GraphicsManager.Names.Length];
        float qW = 190f, qGap = 12f;
        float totalW = qW * images.Length + qGap * (images.Length - 1);
        for (int i = 0; i < images.Length; i++)
        {
            float x = -totalW * 0.5f + qW * 0.5f + i * (qW + qGap);
            var qb = MakeButton(win, "BtnQuality_" + GraphicsManager.Names[i], GraphicsManager.Names[i],
                Ctr, Ctr, new Vector2(x, -60), new Vector2(qW, 70), false, true);
            UnityEventTools.AddIntPersistentListener(qb.onClick, lobbyUI.SetQuality, i);
            images[i] = qb.GetComponent<Image>();
        }
        lobbyUI.qualityButtons = images;
        return panel;
    }

    GameObject BuildMailPanel(Transform root, LobbyUI lobbyUI)
    {
        var panel = MakePanel(root, "MailPanel", "MAIL");
        var win = panel.transform.Find("Window");

        string[] titles = { "Welcome gift", "Starter pack", "Thanks for playing" };
        var labels = new TMP_Text[titles.Length];
        for (int i = 0; i < titles.Length; i++)
        {
            var row = Box(win, "Mail_" + i, new Vector2(0, 120 - i * 84), new Vector2(780, 70), PanelDark);
            Label(row.transform, titles[i] + "   +" + LobbyUI.MailRewards[i] + " GOLD", 28,
                  new Vector2(-130, 0), new Vector2(480, 40), TextAlignmentOptions.Left, Color.white, true);

            var claim = MakeButton(row.transform, "BtnClaim", "CLAIM", Ctr, Ctr,
                new Vector2(270, 0), new Vector2(200, 54), false, true, 26);
            UnityEventTools.AddIntPersistentListener(claim.onClick, lobbyUI.ClaimMail, i);
            labels[i] = claim.transform.Find("Label").GetComponent<TMP_Text>();
        }
        lobbyUI.mailClaimLabels = labels;
        return panel;
    }

    GameObject BuildEventsPanel(Transform root, LobbyUI lobbyUI)
    {
        var panel = MakePanel(root, "EventsPanel", "DAILY REWARD");
        var win = panel.transform.Find("Window");

        int days = LobbyUI.DailyRewards.Length;
        var tiles = new Image[days];
        var rewards = new TMP_Text[days];
        for (int i = 0; i < days; i++)
        {
            float x = -(days - 1) * 110f * 0.5f + i * 110f;
            var tile = Box(win, "Day_" + (i + 1), new Vector2(x, 60), new Vector2(100, 150), PanelDark);
            Label(tile.transform, "DAY " + (i + 1), 24, new Vector2(0, 45), new Vector2(96, 34),
                  TextAlignmentOptions.Center, Color.white, true);
            rewards[i] = Label(tile.transform, "+" + LobbyUI.DailyRewards[i], 28, new Vector2(0, -15),
                  new Vector2(96, 40), TextAlignmentOptions.Center, NeonBlue, true);
            tiles[i] = tile;
        }

        var claim = MakeButton(win, "BtnClaimDaily", "CLAIM", Ctr, Ctr, new Vector2(0, -85),
            new Vector2(300, 70), true, true, 32);
        UnityEventTools.AddPersistentListener(claim.onClick, lobbyUI.ClaimDaily);

        lobbyUI.dayTiles = tiles;
        lobbyUI.dayRewardTexts = rewards;
        lobbyUI.dailyClaimLabel = claim.transform.Find("Label").GetComponent<TMP_Text>();
        return panel;
    }

    // =========================== KOMPONEN ===========================

    // Profil kiri atas (bisa diklik)
    Button BuildTopLeftProfile(Transform root)
    {
        var box = NewImage("Profile", root, slantSprite, PanelDark);
        box.type = Image.Type.Sliced;
        box.raycastTarget = true;
        Place(box.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -30), new Vector2(460, 110));

        var outline = NewImage("Outline", box.transform, slantOutline, Color.white);
        outline.type = Image.Type.Sliced;
        outline.raycastTarget = false;
        Stretch(outline.rectTransform);

        var avatar = NewImage("Avatar", box.transform, null, new Color(0.15f, 0.3f, 0.6f, 1f));
        avatar.raycastTarget = false;
        Place(avatar.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(74, 74));

        var name = NewText("Name", box.transform, playerName, 34);
        name.fontStyle = FontStyles.Bold | FontStyles.Italic;
        name.alignment = TextAlignmentOptions.Left;
        Place(name.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 18), new Vector2(300, 44));

        var lv = NewText("Level", box.transform, "Lv. 1", 26);
        lv.alignment = TextAlignmentOptions.Left;
        lv.color = NeonBlue;
        Place(lv.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, -22), new Vector2(300, 34));

        var btn = box.gameObject.AddComponent<Button>();
        btn.targetGraphic = box;
        btn.transition = Selectable.Transition.ColorTint;
        var c = btn.colors;
        c.normalColor = Color.white;
        c.highlightedColor = new Color(0.65f, 0.9f, 1f);
        c.pressedColor = new Color(0.45f, 0.65f, 0.9f);
        c.selectedColor = Color.white;
        c.fadeDuration = 0.1f;
        btn.colors = c;
        return btn;
    }

    // Tombol cyberpunk: border putih
    Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pivot,
                      Vector2 pos, Vector2 size, bool isPrimary, bool compact = false, float fontSize = 0)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), anchor, pivot, pos, size);

        var bg = go.GetComponent<Image>();
        bg.sprite = slantSprite;
        bg.type = Image.Type.Sliced;
        bg.color = isPrimary ? BrightBlue : PanelDark;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = isPrimary ? new Color(0.2f, 0.8f, 1f, 0.55f) : new Color(0f, 0.1f, 0.3f, 0.5f);
        shadow.effectDistance = new Vector2(0, -5);

        var border = NewImage("Outline", go.transform, slantOutline, Color.white);
        border.type = Image.Type.Sliced;
        border.raycastTarget = false;
        Stretch(border.rectTransform);

        var under = NewImage("Underline", go.transform, null, NeonBlue);
        under.raycastTarget = false;
        Place(under.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8),
              new Vector2(size.x * 0.55f, 3));

        float fs = fontSize > 0 ? fontSize : (compact ? 28 : (isPrimary ? 56 : 32));
        var txt = NewText("Label", go.transform, label, fs);
        txt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        txt.characterSpacing = compact ? 2 : (isPrimary ? 10 : 6);
        txt.color = Color.white;
        Stretch(txt.rectTransform);

        if (!compact)
        {
            var accent = NewImage("Accent", go.transform, null, NeonBlue);
            accent.raycastTarget = false;
            Place(accent.rectTransform, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 0),
                  new Vector2(isPrimary ? 8 : 6, size.y * 0.55f));
            accent.rectTransform.localRotation = Quaternion.Euler(0, 0, -17);

            var arrow = NewText("Arrow", go.transform, "»", isPrimary ? 60 : 38);
            arrow.color = isPrimary ? Color.white : NeonBlue;
            arrow.fontStyle = FontStyles.Bold;
            Place(arrow.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-46, 2), new Vector2(44, 60));
        }

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.transition = Selectable.Transition.ColorTint;
        var c = btn.colors;
        c.normalColor      = Color.white;
        c.highlightedColor = new Color(0.65f, 0.9f, 1f);
        c.pressedColor     = new Color(0.45f, 0.65f, 0.9f);
        c.selectedColor    = Color.white;
        c.fadeDuration     = 0.1f;
        btn.colors = c;
        return btn;
    }

    // Panel pop-up dasar (overlay gelap + jendela + tombol CLOSE)
    GameObject MakePanel(Transform root, string name, string title)
    {
        var overlay = NewImage(name, root, null, new Color(0f, 0.02f, 0.08f, 0.7f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = true; // blok klik di belakang

        var win = NewImage("Window", overlay.transform, slantSprite, new Color(0.03f, 0.1f, 0.26f, 0.95f));
        win.type = Image.Type.Sliced;
        Place(win.rectTransform, Ctr, Ctr, Vector2.zero, new Vector2(900, 580));

        var outline = NewImage("Outline", win.transform, slantOutline, Color.white);
        outline.type = Image.Type.Sliced;
        outline.raycastTarget = false;
        Stretch(outline.rectTransform);

        var t = NewText("Title", win.transform, title, 56);
        t.fontStyle = FontStyles.Bold | FontStyles.Italic;
        t.characterSpacing = 8;
        Place(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(700, 80));

        var close = MakeButton(win.transform, "BtnClose", "CLOSE", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 36), new Vector2(300, 76), false);
        UnityEventTools.AddBoolPersistentListener(close.onClick, overlay.gameObject.SetActive, false);
        return overlay.gameObject;
    }

    // Kotak miring dengan outline tipis (untuk kartu, slot, baris, dll.)
    Image Box(Transform parent, string name, Vector2 pos, Vector2 size, Color fill)
    {
        var img = NewImage(name, parent, slantSprite, fill);
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        Place(img.rectTransform, Ctr, Ctr, pos, size);

        var o = NewImage("Outline", img.transform, slantOutline, new Color(1f, 1f, 1f, 0.55f));
        o.type = Image.Type.Sliced;
        o.raycastTarget = false;
        Stretch(o.rectTransform);
        return img;
    }

    TextMeshProUGUI Label(Transform parent, string text, float size, Vector2 pos, Vector2 box,
                          TextAlignmentOptions align, Color color, bool bold = false)
    {
        var t = NewText("Text", parent, text, size);
        t.alignment = align;
        t.color = color;
        if (bold) t.fontStyle = FontStyles.Bold | FontStyles.Italic;
        Place(t.rectTransform, Ctr, Ctr, pos, box);
        return t;
    }

    Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), Ctr, Ctr, pos, size);

        var back = NewImage("Background", go.transform, null, new Color(1f, 1f, 1f, 0.15f));
        back.raycastTarget = false;
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
        fill.raycastTarget = false;
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
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 1f;
        return s;
    }

    // --- PEMBUAT SPRITE (Jajar Genjang) ---
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
        img.color = c;
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
