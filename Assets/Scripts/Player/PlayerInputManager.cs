using Managers;
using Reflex.Attributes;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputManager : NetworkBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private PlayerCharacterController controller;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;

        [Inject] private PlayerCamera _cam;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cam.SetFPTarget(head);
        }

        private void Update()
        {
            if (!IsOwner) return;

            controller.SetInput(new PlayerInput()
            {
                Movement = OnScreenUIManager.Instance.ShouldLockInput
                    ? Vector2.zero
                    : moveAction.action.ReadValue<Vector2>(),
                Rotation = _cam.transform.rotation,
                JumpDown = !OnScreenUIManager.Instance.ShouldLockInput && jumpAction.action.IsPressed(),
                SprintDown = !OnScreenUIManager.Instance.ShouldLockInput && sprintAction.action.IsPressed(),
            });
        }
    }
}