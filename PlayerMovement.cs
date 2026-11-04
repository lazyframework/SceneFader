using UnityEngine;
using UnityEngine.InputSystem; // Wajib ditambahkan untuk Input System Baru

namespace IdyllicFantasyNature
{
    public class PlayerMovement : MonoBehaviour
    {
        [Range(1f, 20f)]
        [SerializeField] private float _movementSpeed;
        [Tooltip("run multiplier of the movement speed")]
        [Range(1f, 20f)]
        [SerializeField] private float _runMultiplier;
        [SerializeField] private float _gravity = -9.81f;
        [Range(1f, 20f)]
        [SerializeField] private float _jumpHeight;

        private CharacterController characterController;
        Vector3 _controllerVelocity;

        // TAMBAHAN UNTUK MOBILE (Agar nanti bisa disambung ke Joystick)
        [HideInInspector] public Vector2 mobileMoveInput;
        [HideInInspector] public bool mobileJumpPressed;
        [HideInInspector] public bool mobileRunPressed;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
        }

        void Update()
        {
            // stops the y velocity when player is on the ground and the velocity has reached 0
            if (characterController.isGrounded && _controllerVelocity.y < 0)
            {
                _controllerVelocity.y = 0;
            }

            // --- BAGIAN YANG DIUBAH: INPUT SYSTEM BARU ---
            float moveX = 0f;
            float moveZ = 0f;
            bool jumpPressed = false;
            bool runPressed = false;

            // Cek apakah ada Keyboard yang terhubung (untuk tes di PC)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                if (Keyboard.current.dKey.isPressed) moveX = 1f;
                if (Keyboard.current.sKey.isPressed) moveZ = -1f;
                if (Keyboard.current.wKey.isPressed) moveZ = 1f;
                
                jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
                runPressed = Keyboard.current.leftShiftKey.isPressed;
            }

            // Gabungkan dengan input Mobile (Joystick)
            if (mobileMoveInput.magnitude > 0.1f)
            {
                moveX = mobileMoveInput.x;
                moveZ = mobileMoveInput.y;
            }
            if (mobileJumpPressed) jumpPressed = true;
            if (mobileRunPressed) runPressed = true;
            // ----------------------------------------------

            // moves the controller in the desired direction on the x- and z-axis
            Vector3 movement = transform.right * moveX + transform.forward * moveZ;
            characterController.Move(movement * _movementSpeed * Time.deltaTime);

            // gravity affects the controller on the y-axis
            _controllerVelocity.y += _gravity * Time.deltaTime;

            // moves the controller on the y-axis
            characterController.Move(_controllerVelocity * Time.deltaTime);

            // the controller is able to jump when on the ground
            if (jumpPressed && characterController.isGrounded)
            {
                _controllerVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                mobileJumpPressed = false; // Reset tombol mobile
            }

            // the controller is able to run
            if (runPressed)
            {
                characterController.Move(movement * Time.deltaTime * _runMultiplier);
            }
        }
    }
}
