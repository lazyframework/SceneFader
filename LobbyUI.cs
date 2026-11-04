using UnityEngine;

// Taruh di Assets/Scripts (BUKAN folder Editor).
// Dipasang otomatis ke MenuManager oleh MainMenuBuilder.
public class LobbyUI : MonoBehaviour
{
    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }
}
