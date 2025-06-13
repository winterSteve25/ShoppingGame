using Items;
using KinematicCharacterController;
using Managers;
using Objective;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerHandInteraction : NetworkBehaviour
    {
        [Header("References")] 
        [SerializeField] private NetworkInventory inventory;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        [Header("Parameters")] 
        [SerializeField] private float range;
        [SerializeField] private float shoveForce = 80;
        [SerializeField] private AnimationCurve throwMultiplierCurve;

        /// NOT SYNCED ONLY AVAILABLE ON OWNER
        private Transform _fpCam;
        private WorldItem _leftHandItem;
        private WorldItem _rightHandItem;
        
        private bool _throwing;
        private float _heldTime;

        private void Start()
        {
            _fpCam = CameraManager.Current.FPCam.transform;
        }

        private void Update()
        {
            if (!IsOwner) return;
            HandleHandInput(Mouse.current.leftButton, true);
            HandleHandInput(Mouse.current.rightButton, false);
        }

        private void HandleHandInput(ButtonControl button, bool left)
        {
            ref var handItem = ref left ? ref _leftHandItem : ref _rightHandItem;

            if (handItem == null)
            {
                if (!button.wasPressedThisFrame) return;
                var didHit = Physics.Raycast(head.position, _fpCam.forward, out var hit, range);
                if (!didHit)
                {
                    Punch(false, hit, left);
                    return;
                }

                if (!hit.transform.TryGetComponent(out WorldItem item))
                {
                    if (hit.transform.TryGetComponent(out IInteractable interactable))
                    {
                        if (interactable.Interact(NetworkObject)) return;
                    }
                    
                    Punch(true, hit, left);
                    return;
                }

                if (item.NetworkObject.OwnerClientId == NetworkManager.LocalClientId)
                {
                    PickupItem(item, left);
                }
                else
                {
                    PickUpItemAndChangeOwnerRpc(item, left);
                }
            }
            else
            {
                if (button.wasPressedThisFrame)
                {
                    var didHit = Physics.Raycast(head.position, _fpCam.forward, out var hit, range);
                    if (didHit && hit.transform.TryGetComponent(out IItemTaker interactable))
                    {
                        if (interactable.Submit(handItem))
                        {
                            handItem = null;
                        }

                        return;
                    }
                }

                if (button.wasPressedThisFrame && !_throwing)
                {
                    _throwing = true;
                }

                if (button.wasReleasedThisFrame && _throwing)
                {
                    _throwing = false;
                    if (handItem.Drop())
                    {
                        if (handItem.TryGetComponent(out Rigidbody rb))
                        {
                            rb.AddForce(
                                (_fpCam.forward + new Vector3(0, 0.2f, 0)) * throwMultiplierCurve.Evaluate(_heldTime),
                                ForceMode.VelocityChange
                            );
                        }

                        handItem = null;
                    }
                }

                if (_throwing)
                {
                    _heldTime += Time.deltaTime;
                }
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

        private void Punch(bool didHit, RaycastHit hit, bool left)
        {
            if (!didHit) return;
            if (!hit.transform.TryGetComponent(out PlayerHandInteraction no)) return;
            ApplyForceRpc(no, shoveForce, RpcTarget.Single(no.OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplyForceRpc(NetworkBehaviourReference obj, float force, RpcParams _)
        {
            if (!obj.TryGet(out PlayerHandInteraction o, NetworkManager)) return;
            if (!o.TryGetComponent(out KinematicCharacterMotor motor)) return;
            motor.BaseVelocity += transform.forward * (force * (motor.GroundingStatus.IsStableOnGround ? 1.2f : 1));

            DropItemAndApplyForce(ref o._leftHandItem, transform.forward * force);
            DropItemAndApplyForce(ref o._rightHandItem, transform.forward * force);
        }

        private void DropItemAndApplyForce(ref WorldItem item, Vector3 force)
        {
            if (item == null || !item.Drop()) return;
            if (item.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(force * 0.1f, ForceMode.VelocityChange);
            }

            item = null;
        }
    }
}