using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }
}
