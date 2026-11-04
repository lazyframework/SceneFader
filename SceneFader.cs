using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;
    public Image fadeImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        
        // Fade Out (Hitam)
        for (float t = 0; t < 1; t += Time.deltaTime * 2f)
        {
            color.a = Mathf.Lerp(0, 1, t);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1;
        fadeImage.color = color;

        // Load Scene
        SceneManager.LoadScene(sceneName);
    }
}
