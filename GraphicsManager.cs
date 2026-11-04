using UnityEngine;
using UnityEngine.SceneManagement;

// Taruh di Assets/Scripts (BUKAN folder Editor).
// Otomatis aktif sejak game start, jadi pilihan graphic dari lobby
// langsung berlaku di GameScene (tidak perlu dipasang ke objek apa pun).
public static class GraphicsManager
{
    public const string Key = "GraphicsQuality";
    public static readonly string[] Names = { "LOW", "MEDIUM", "HIGH", "ULTRA" };

    // Batas FPS per level: Low, Medium, High, Ultra (ubah sesuka hati)
    static readonly int[] Fps = { 30, 45, 60, 90 };

    // Default = HIGH (index 2)
    public static int Current => Mathf.Clamp(PlayerPrefs.GetInt(Key, 2), 0, 3);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Apply(Current);
        SceneManager.sceneLoaded += (scene, mode) => Apply(Current);
    }

    public static void Set(int level)
    {
        level = Mathf.Clamp(level, 0, 3);
        PlayerPrefs.SetInt(Key, level);
        PlayerPrefs.Save();
        Apply(level);
    }

    static void Apply(int level)
    {
        // Petakan 4 level ke Quality Level yang ada di Project Settings > Quality
        int count = QualitySettings.names.Length;
        int index = Mathf.RoundToInt(level / 3f * (count - 1));
        QualitySettings.SetQualityLevel(index, true);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Fps[level];
    }
}
