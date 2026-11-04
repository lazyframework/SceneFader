using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelSettings;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string shopSceneName = "ShopScene";

    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
    }

    public void StartGame()     => SceneFader.Instance.LoadScene(gameSceneName);
    public void OpenShop()      => SceneFader.Instance.LoadScene(shopSceneName);
    public void OpenSettings()  => panelSettings.SetActive(true);
    public void CloseSettings() => panelSettings.SetActive(false);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
