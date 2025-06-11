using Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerPickup : NetworkBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private NetworkInventory inventory;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        private Transform _fpCam;
        private WorldItem _leftHandItem;
        private WorldItem _rightHandItem;

        private void Start()
        {
            _fpCam = CameraManager.Current.FPCam.transform;
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_leftHandItem == null)
                {
                    if (!Physics.Raycast(head.position, _fpCam.forward, out var hit, range)) return;
                    if (!hit.transform.TryGetComponent(out WorldItem item)) return;
                    if (!item.PickUp(inventory, NetworkObject, leftHand.name)) return;
                    _leftHandItem = item;
                }
                else if (_leftHandItem.Drop())
                {
                    _leftHandItem = null;
                }
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (_rightHandItem == null)
                {
                    if (!Physics.Raycast(head.position, _fpCam.forward, out var hit, range)) return;
                    if (!hit.transform.TryGetComponent(out WorldItem item)) return;
                    if (!item.PickUp(inventory, NetworkObject, rightHand.name)) return;
                    _rightHandItem = item;
                }
                else if (_rightHandItem.Drop())
                {
                    _rightHandItem = null;
                }
            }
        }
    }
}