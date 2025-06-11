using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class NetworkPlayerInputManager : NetworkBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private PlayerCharacterController controller;
        [SerializeField] private InputActionAsset inputAsset;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private GameObject _cam;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _moveAction = inputAsset.FindAction("Player/Move");
            _jumpAction = inputAsset.FindAction("Player/Jump");
            _cam = CameraManager.Current.FPCam.gameObject;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            CameraManager.Current.SetFPTarget(head);
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            var movement = _moveAction.ReadValue<Vector2>();
            var jump = _jumpAction.WasPressedThisFrame();
            
            controller.SetInput(new PlayerInput()
            {
                Movement = movement,
                Rotation = _cam.transform.rotation,
                JumpDown = jump,
            });
        }
    }
}