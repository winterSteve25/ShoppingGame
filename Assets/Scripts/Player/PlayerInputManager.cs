using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerInputManager : NetworkBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private PlayerCharacterController controller;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        
        private GameObject _cam;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _cam = CameraManager.Current.FPCam.gameObject;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            CameraManager.Current.SetFPTarget(head);
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            controller.SetInput(new PlayerInput()
            {
                Movement = moveAction.action.ReadValue<Vector2>(),
                Rotation = _cam.transform.rotation,
                JumpDown = jumpAction.action.IsPressed(),
                SprintDown = sprintAction.action.IsPressed(),
            });

            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}