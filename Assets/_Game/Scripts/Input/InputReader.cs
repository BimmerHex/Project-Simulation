using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Game.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IPlayerActions
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public float RotateInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsPrecision { get; private set; }

        public event UnityAction OnPrimaryActionPerformed;
        public event UnityAction OnSecondaryActionPerformed;
        public event UnityAction OnInteractPerformed;
        public event UnityAction OnJumpPerformed;

        private GameInput _gameInput;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.Player.SetCallbacks(this);
            }
            
            _gameInput.Player.Enable();
        }

        private void OnDisable()
        {
            _gameInput?.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context) => MoveInput = context.ReadValue<Vector2>();

        public void OnLook(InputAction.CallbackContext context) => LookInput = context.ReadValue<Vector2>();

        public void OnRotate(InputAction.CallbackContext context) => RotateInput = context.ReadValue<float>();

        public void OnSprint(InputAction.CallbackContext context) => IsSprinting = context.ReadValueAsButton();

        public void OnPrecision(InputAction.CallbackContext context) => IsPrecision = context.ReadValueAsButton();

        public void OnPrimaryAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnPrimaryActionPerformed?.Invoke();
        }

        public void OnSecondaryAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnSecondaryActionPerformed?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) OnInteractPerformed?.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) OnJumpPerformed?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            // Not implemented
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            // Not implemented
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            // Not implemented
        }
    }
}
