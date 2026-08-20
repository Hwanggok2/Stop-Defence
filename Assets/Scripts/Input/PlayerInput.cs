using Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class PlayerInput : MonoBehaviour, PlayerInputAction.IPlayerActions
    {
        private PlayerInputAction input;
        private bool isDragging;

        private void OnEnable()
        {
            if (input == null)
                input = new PlayerInputAction();

            input.Player.SetCallbacks(this);
            input.Player.Enable();
        }

        public void OnDrag(InputAction.CallbackContext context)
        {
            if (context.started)  isDragging = true;
            if (context.canceled) isDragging = false;
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            if (!isDragging) return;
            CameraController.Instance.Move(context.ReadValue<Vector2>());
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<Vector2>().y;
            CameraController.Instance.Zoom(scroll);
        }
    }
}