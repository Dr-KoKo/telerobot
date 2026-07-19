using Telerobot.Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telerobot.Game.Runtime
{
    public sealed class InputSystemPlayerInput : MonoBehaviour, IPlayerInput
    {
        private InputActionAsset actions;
        private InputAction move;
        private InputAction look;
        private InputAction attack;
        private InputAction interact;
        private InputAction jump;

        public void Initialize(InputActionAsset source)
        {
            if (source == null) return;
            actions = Instantiate(source);
            move = actions.FindAction("Player/Move", true);
            look = actions.FindAction("Player/Look", true);
            attack = actions.FindAction("Player/Attack", true);
            interact = actions.FindAction("Player/Interact", false);
            jump = actions.FindAction("Player/Jump", false);
            actions.Enable();
        }

        private void OnDestroy()
        {
            if (actions == null) return;
            actions.Disable();
            Destroy(actions);
        }

        public PlayerInputFrame ReadFrame()
        {
            if (actions == null) return default;
            var moveValue = move.ReadValue<Vector2>();
            var lookValue = look.ReadValue<Vector2>();
            var keyboard = Keyboard.current;
            return new PlayerInputFrame
            {
                Move = new Float2(moveValue.x, moveValue.y),
                Look = new Float2(lookValue.x, lookValue.y),
                FirePressed = attack.WasPressedThisFrame(),
                ReloadPressed = keyboard != null && keyboard.rKey.wasPressedThisFrame,
                GrenadePressed = keyboard != null && keyboard.gKey.wasPressedThisFrame,
                InteractPressed = (interact != null && interact.WasPressedThisFrame()) || keyboard != null && keyboard.eKey.wasPressedThisFrame,
                JumpPressed = (jump != null && jump.WasPressedThisFrame()) || keyboard != null && keyboard.spaceKey.wasPressedThisFrame,
                SprintHeld = keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed),
                TogglePerspectivePressed = keyboard != null && keyboard.vKey.wasPressedThisFrame,
                PausePressed = keyboard != null && keyboard.escapeKey.wasPressedThisFrame
            };
        }
    }
}
