using Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Player
{
    public class PlayerPickup : NetworkBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private NetworkInventory inventory;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        /// NOT SYNCED
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
            HandleHandInput(Mouse.current.leftButton, ref _leftHandItem, true);
            HandleHandInput(Mouse.current.rightButton, ref _rightHandItem, false);
        }

        private void HandleHandInput(ButtonControl button, ref WorldItem handItem, bool left)
        {
            if (!button.wasPressedThisFrame) return;
            if (handItem == null)
            {
                if (!Physics.Raycast(head.position, _fpCam.forward, out var hit, range)) return;
                if (!hit.transform.TryGetComponent(out WorldItem item)) return;

                if (item.NetworkObject.OwnerClientId == NetworkManager.LocalClientId)
                {
                    PickupItem(item, left);
                }
                else
                {
                    PickUpItemAndChangeOwnerRpc(item, left);
                }
            }
            else if (handItem.Drop())
            {
                handItem = null;
            }
        }

        [Rpc(SendTo.Server)]
        private void PickUpItemAndChangeOwnerRpc(NetworkBehaviourReference obj, bool left, RpcParams param = default)
        {
            if (!obj.TryGet(out WorldItem o, NetworkManager)) return;
            o.NetworkObject.ChangeOwnership(param.Receive.SenderClientId);
            OwnershipChangedRpc(o, left, RpcTarget.Single(param.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void OwnershipChangedRpc(NetworkBehaviourReference obj, bool left, RpcParams _)
        {
            if (!obj.TryGet(out WorldItem item, NetworkManager)) return;
            PickupItem(item, left);
        }

        private void PickupItem(WorldItem item, bool left)
        {
            if (!item.PickUp(inventory, left ? leftHand : rightHand)) return;
            if (left)
            {
                _leftHandItem = item;
            }
            else
            {
                _rightHandItem = item;
            }
        }
    }
}