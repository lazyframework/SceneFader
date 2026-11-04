using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDBuilder : EditorWindow
{
    Sprite profileSprite;
    Sprite hpBarSprite;
    Sprite mpBarSprite;

    [MenuItem("Tools/Bangun HUD GameScene")]
    static void Open() => GetWindow<HUDBuilder>("HUD Builder");

    void OnGUI()
    {
        GUILayout.Label("Aset (Opsional)", EditorStyles.boldLabel);
        profileSprite = (Sprite)EditorGUILayout.ObjectField("Foto Profil", profileSprite, typeof(Sprite), false);
        hpBarSprite = (Sprite)EditorGUILayout.ObjectField("Sprite Bar HP", hpBarSprite, typeof(Sprite), false);
        mpBarSprite = (Sprite)EditorGUILayout.ObjectField("Sprite Bar MP", mpBarSprite, typeof(Sprite), false);

        GUILayout.Space(10);
        if (GUILayout.Button("BANGUN HUD", GUILayout.Height(35))) BuildHUD();
    }

    void BuildHUD()
    {
        // Cari atau buat Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("MobileUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Cek jika HUD sudah ada
        if (GameObject.Find("HUD_Container") != null)
        {
            EditorUtility.DisplayDialog("Error", "HUD sudah ada di Canvas!", "OK");
            return;
        }

        // 1. Container Utama HUD
        var hudContainer = new GameObject("HUD_Container", typeof(RectTransform));
        hudContainer.transform.SetParent(canvas.transform, false);
        var hudRt = hudContainer.GetComponent<RectTransform>();
        hudRt.anchorMin = Vector2.zero;
        hudRt.anchorMax = Vector2.one;
        hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;

        // 2. Panel Karakter (Kiri Atas)
        var charPanel = new GameObject("CharPanel", typeof(RectTransform), typeof(Image));
        charPanel.transform.SetParent(hudContainer.transform, false);
        var cpRt = charPanel.GetComponent<RectTransform>();
        cpRt.anchorMin = cpRt.anchorMax = new Vector2(0, 1);
        cpRt.pivot = new Vector2(0, 1);
        cpRt.anchoredPosition = new Vector2(20, -20);
        cpRt.sizeDelta = new Vector2(400, 120);
        charPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f); // Latar transparan gelap

        // Foto Profil
        var profile = NewImage("ProfilePic", charPanel.transform, profileSprite, Color.white);
        profile.rectTransform.anchoredPosition = new Vector2(60, -60);
        profile.rectTransform.sizeDelta = new Vector2(100, 100);

        // Teks Level
        var txtLv = NewText("TxtLevel", charPanel.transform, "Lv. 1", 24);
        txtLv.rectTransform.anchoredPosition = new Vector2(180, -20);
        txtLv.alignment = TextAlignmentOptions.Left;

        // Bar HP & MP
        CreateBar(charPanel.transform, "HPBar", new Vector2(180, -50), new Color(0.8f, 0.1f, 0.1f), hpBarSprite, "100 / 100");
        CreateBar(charPanel.transform, "MPBar", new Vector2(180, -90), new Color(0.1f, 0.4f, 0.9f), mpBarSprite, "100 / 100");

        // 3. Minimap (Kanan Atas)
        var minimap = NewImage("MinimapBG", hudContainer.transform, null, new Color(0, 0, 0, 0.5f));
        minimap.rectTransform.anchorMin = minimap.rectTransform.anchorMax = new Vector2(1, 1);
        minimap.rectTransform.pivot = new Vector2(1, 1);
        minimap.rectTransform.anchoredPosition = new Vector2(-20, -20);
        minimap.rectTransform.sizeDelta = new Vector2(250, 250);
        
        // Teks Nama Area
        var txtArea = NewText("TxtArea", minimap.transform, "Cartoon City", 20);
        txtArea.rectTransform.anchoredPosition = new Vector2(0, -130);
        txtArea.color = Color.white;

        // 4. Menu Kanan Atas (Deretan Tombol)
        var menuContainer = new GameObject("TopMenu", typeof(RectTransform));
        menuContainer.transform.SetParent(hudContainer.transform, false);
        var mcRt = menuContainer.GetComponent<RectTransform>();
        mcRt.anchorMin = mcRt.anchorMax = new Vector2(1, 1);
        mcRt.pivot = new Vector2(1, 1);
        mcRt.anchoredPosition = new Vector2(-300, -20);
        mcRt.sizeDelta = new Vector2(400, 100);

        // Buat 4 Tombol (Reward, Shop, Bag, Menu)
        CreateTopMenuButton(menuContainer.transform, "BtnReward", "Reward", 0);
        CreateTopMenuButton(menuContainer.transform, "BtnShop", "Shop", 1);
        CreateTopMenuButton(menuContainer.transform, "BtnBag", "Bag", 2);
        CreateTopMenuButton(menuContainer.transform, "BtnMenu", "Menu", 3);

        Selection.activeGameObject = hudContainer;
        Debug.Log("HUD berhasil dibuat!");
    }

    // --- FUNGSI BANTUAN ---
    
    void CreateBar(Transform parent, string name, Vector2 pos, Color fillColor, Sprite bgSprite, string defaultText)
    {
        var barBg = NewImage(name + "_BG", parent, bgSprite, new Color(0.2f, 0.2f, 0.2f, 0.8f));
        barBg.rectTransform.anchoredPosition = pos;
        barBg.rectTransform.sizeDelta = new Vector2(250, 25);

        var fillArea = new GameObject(name + "_FillArea", typeof(RectTransform));
        fillArea.transform.SetParent(barBg.transform, false);
        var faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(2, 2); // Padding
        faRt.offsetMax = new Vector2(-2, -2);

        var fill = NewImage("Fill", fillArea.transform, null, fillColor);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

        var txt = NewText("Text", barBg.transform, defaultText, 18);
        txt.rectTransform.anchoredPosition = Vector2.zero;
        txt.rectTransform.sizeDelta = new Vector2(250, 25);
    }

    void CreateTopMenuButton(Transform parent, string name, string label, int index)
    {
        var btn = NewImage(name, parent, null, Color.clear);
        btn.gameObject.AddComponent<Button>();
        var rt = btn.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-(index * 90), 0);
        rt.sizeDelta = new Vector2(80, 80);

        var icon = NewImage("Icon", btn.transform, null, Color.white);
        icon.rectTransform.anchoredPosition = new Vector2(0, 10);
        icon.rectTransform.sizeDelta = new Vector2(50, 50);

        var txt = NewText("Txt", btn.transform, label, 16);
        txt.rectTransform.anchoredPosition = new Vector2(0, -25);
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
        return t;
    }
}
