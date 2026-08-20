using GameInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public class PlayerInput : MonoBehaviour, PlayerInputAction.IPlayerActions
    {
        [Header("Keyboard Camera")]
        [SerializeField] private float keyboardMoveSpeed = 5f;

        private PlayerInputAction input;
        private bool isDragging;

        private Vector2 keyMoveInput;

        private void OnEnable()
        {
            if (input == null)
                input = new PlayerInputAction();

            input.Player.SetCallbacks(this);
            input.Player.Enable();
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.Player.RemoveCallbacks(this);
                input.Player.Disable();
            }
        }

        private void Update()
        {
            if (keyMoveInput == Vector2.zero)
                return;

            CameraController.Instance.MoveKeyboard(
                keyMoveInput,
                keyboardMoveSpeed * Time.deltaTime
            );
        }

        public void OnDrag(InputAction.CallbackContext context)
        {
            if (context.started)
                isDragging = true;

            if (context.canceled)
                isDragging = false;
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            if (!isDragging)
                return;

            CameraController.Instance.Move(
                context.ReadValue<Vector2>()
            );
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<Vector2>().y;
            CameraController.Instance.Zoom(scroll);
        }

        public void OnKeyMove(InputAction.CallbackContext context)
        {
            keyMoveInput = context.ReadValue<Vector2>();
        }
    }
}