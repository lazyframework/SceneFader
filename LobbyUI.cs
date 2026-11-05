using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Taruh di Assets/Scripts (BUKAN folder Editor).
// Dipasang otomatis ke MenuManager oleh MainMenuBuilder.
public class LobbyUI : MonoBehaviour
{
    // Referensi diisi otomatis oleh MainMenuBuilder
    public Image[] qualityButtons;
    public Image[] characterCards;
    public TMP_Text goldText;
    public TMP_Text[] mailClaimLabels;
    public Image[] dayTiles;
    public TMP_Text[] dayRewardTexts;
    public TMP_Text dailyClaimLabel;

    // Hadiah (ubah sesuka hati)
    public static readonly int[] DailyRewards = { 100, 150, 200, 250, 300, 400, 1000 };
    public static readonly int[] MailRewards  = { 500, 300, 200 };

    const string GoldKey = "Gold", CharKey = "SelectedCharacter";
    const string LastKey = "DailyLast", StreakKey = "DailyStreak", MailKey = "MailClaimed_";

    static readonly Color On   = new Color(0.87f, 0.18f, 0.26f);          // merah
    static readonly Color Off  = new Color(0.07f, 0.10f, 0.22f, 0.93f);   // navy
    static readonly Color Done = new Color(0.10f, 0.12f, 0.20f, 0.6f);

    // Bisa dibaca dari GameScene: LobbyUI.Gold, LobbyUI.SelectedCharacter
    public static int Gold => PlayerPrefs.GetInt(GoldKey, 0);
    public static int SelectedCharacter => PlayerPrefs.GetInt(CharKey, 0);

    void Start() => Refresh();

    // ---------- Settings ----------
    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    // 0 = Low, 1 = Medium, 2 = High, 3 = Ultra
    public void SetQuality(int level)
    {
        GraphicsManager.Set(level);
        Refresh();
    }

    // ---------- Karakter ----------
    public void SelectCharacter(int index)
    {
        PlayerPrefs.SetInt(CharKey, index);
        PlayerPrefs.Save();
        Refresh();
    }

    // ---------- Mail ----------
    static bool IsMailClaimed(int i) => PlayerPrefs.GetInt(MailKey + i, 0) == 1;

    public void ClaimMail(int i)
    {
        if (i < 0 || i >= MailRewards.Length || IsMailClaimed(i)) return;
        AddGold(MailRewards[i]);
        PlayerPrefs.SetInt(MailKey + i, 1);
        PlayerPrefs.Save();
        Refresh();
    }

    // ---------- Events: hadiah login harian ----------
    static string Today => DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    static bool CanClaimDaily => PlayerPrefs.GetString(LastKey, "") != Today;

    // Jumlah hari yang sudah diklaim di siklus 7 hari (reset kalau bolong sehari)
    static int Streak()
    {
        int s = PlayerPrefs.GetInt(StreakKey, 0);
        string last = PlayerPrefs.GetString(LastKey, "");
        if (last == Today) return s;
        if (last != "" && DateTime.TryParseExact(last, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d))
        {
            if ((DateTime.Now.Date - d.Date).Days > 1) return 0;
        }
        return s >= DailyRewards.Length ? 0 : s;
    }

    public void ClaimDaily()
    {
        if (!CanClaimDaily) return;
        int s = Streak();
        AddGold(DailyRewards[s]);
        PlayerPrefs.SetInt(StreakKey, s + 1);
        PlayerPrefs.SetString(LastKey, Today);
        PlayerPrefs.Save();
        Refresh();
    }

    static void AddGold(int amount)
    {
        PlayerPrefs.SetInt(GoldKey, Gold + amount);
    }

    // ---------- Update tampilan ----------
    void Refresh()
    {
        if (goldText != null) goldText.text = Gold.ToString("N0", CultureInfo.InvariantCulture);

        if (qualityButtons != null)
        {
            int q = GraphicsManager.Current;
            for (int i = 0; i < qualityButtons.Length; i++)
                if (qualityButtons[i] != null) qualityButtons[i].color = (i == q) ? On : Off;
        }

        if (characterCards != null)
        {
            int c = SelectedCharacter;
            for (int i = 0; i < characterCards.Length; i++)
                if (characterCards[i] != null) characterCards[i].color = (i == c) ? On : Off;
        }

        if (mailClaimLabels != null)
            for (int i = 0; i < mailClaimLabels.Length; i++)
                if (mailClaimLabels[i] != null)
                    mailClaimLabels[i].text = IsMailClaimed(i) ? "CLAIMED" : "CLAIM";

        if (dayTiles != null)
        {
            int streak = Streak();
            bool can = CanClaimDaily;
            for (int i = 0; i < dayTiles.Length; i++)
            {
                if (dayTiles[i] == null) continue;
                bool claimed = i < streak;
                bool today = can && i == streak;
                dayTiles[i].color = claimed ? Done : (today ? On : Off);
                if (dayRewardTexts != null && i < dayRewardTexts.Length && dayRewardTexts[i] != null)
                    dayRewardTexts[i].text = claimed ? "DONE" : "+" + DailyRewards[i];
            }
            if (dailyClaimLabel != null)
                dailyClaimLabel.text = can ? "CLAIM" : "CLAIMED";
        }
    }
}
