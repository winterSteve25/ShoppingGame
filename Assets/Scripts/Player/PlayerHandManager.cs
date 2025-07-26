using System.Collections.Generic;
using Items;
using KinematicCharacterController;
using Managers;
using Objective;
using Reflex.Attributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player
{
    public class PlayerHandManager : NetworkBehaviour
    {
        [Header("References")] 
        [SerializeField] private NetworkInventory inventory;
        [SerializeField] private Transform head;
        [SerializeField] private PlayerHand leftHand;
        [SerializeField] private PlayerHand rightHand;

        [Header("Parameters")] 
        [SerializeField] private float range;
        [SerializeField] private float shoveForce = 80;
        [SerializeField] private float shoveCooldown = 0.8f;
        [SerializeField] private float timeToRemoveItemFromTree = 1f;
        [SerializeField] private AnimationCurve throwMultiplierCurve;
        
        public Stack<IInteractableArea> InteractableAreas => _interactableAreas;
        public Transform Head => head;
        public Transform FpCam => _fpCam.transform;
        public AnimationCurve ThrowMultiplierCurve => throwMultiplierCurve;
        public NetworkInventory Inventory => inventory;
        public float Range => range;
        public float ShoveForce => shoveForce;
        public float ShoveCooldown => shoveCooldown;
        public float TimeToRemoveItemFromTree => timeToRemoveItemFromTree;
        public WorldItem LeftHandItem => leftHand.itemHeld;
        public WorldItem RightHandItem => rightHand.itemHeld;
        public Slider InteractionProgressSlider => _interactionProgressSlider;

        /// NOT SYNCED ONLY AVAILABLE ON OWNER
        [Inject] private PlayerCamera _fpCam;
        [Inject] private Slider _interactionProgressSlider;
        private Stack<IInteractableArea> _interactableAreas;
        private float _superStrengthMultiplier;
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _interactableAreas = new Stack<IInteractableArea>();
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (leftHand.UpdateHand(Mouse.current.leftButton, _superStrengthMultiplier))
            {
                _superStrengthMultiplier = 1;
            }

            if (rightHand.UpdateHand(Mouse.current.rightButton, _superStrengthMultiplier))
            {
                _superStrengthMultiplier = 1;
            }
        }

        public void PickupItemToHand(WorldItem item, bool left)
        {
            if (item.NetworkObject.OwnerClientId == NetworkManager.LocalClientId)
            {
                if (left)
                {
                    leftHand.PickupItem(item);
                }
                else
                {
                    rightHand.PickupItem(item);
                }
            }
            else
            {
                PickUpItemAndChangeOwnerRpc(item, left);
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
            PickupItemToHand(item, left);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void ApplyForceRpc(NetworkBehaviourReference obj, Vector3 force, RpcParams _)
        {
            if (!obj.TryGet(out PlayerHandManager o, NetworkManager)) return;
            if (!o.TryGetComponent(out KinematicCharacterMotor motor)) return;
            motor.BaseVelocity += (force * (motor.GroundingStatus.IsStableOnGround ? 1.2f : 1));

            DropItemAndApplyForce(ref o.leftHand.itemHeld, force);
            DropItemAndApplyForce(ref o.rightHand.itemHeld, force);
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

        private void OnTriggerEnter(Collider other)
        {
            if (!IsOwner) return;
            if (!other.TryGetComponent(out IInteractableArea area)) return;
            _interactableAreas.Push(area);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsOwner) return;
            if (!other.TryGetComponent(out IInteractableArea _)) return;
            _interactableAreas.Pop();
        }

        [Rpc(SendTo.Owner)]
        public void MultiplySuperStrengthMultiplierRpc(float factor)
        {
            _superStrengthMultiplier *= factor;
        }
    }
}