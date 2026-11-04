using UnityEngine;
using StarterAssets; // Sesuaikan jika kamu memakai script karakter lain

public class MobileInputBridge : MonoBehaviour
{
    public FixedJoystick joystick;
    public StarterAssetsInputs playerInputs;

    void Update()
    {
        if (joystick != null && playerInputs != null)
        {
            // Mengirim input joystick ke karakter
            playerInputs.move = new Vector2(joystick.Horizontal, joystick.Vertical);
        }
    }
}
