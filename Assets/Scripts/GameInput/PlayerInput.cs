using GameInput;
using UnityEngine;

namespace GameInput
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

        public void OnDrag(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.started)  isDragging = true;
            if (context.canceled) isDragging = false;
        }

        public void OnMouseMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!isDragging) return;
            CameraController.Instance.Move(context.ReadValue<Vector2>());
        }

        public void OnZoom(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<Vector2>().y;
            CameraController.Instance.Zoom(scroll);
        }
    }
}