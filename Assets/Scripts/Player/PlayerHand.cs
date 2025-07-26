using Items;
using Objective;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerHand : NetworkBehaviour
    {
        [SerializeField] private PlayerHandManager manager;
        [SerializeField] private Transform position;
        [SerializeField] private bool isLeftHand;

        public WorldItem itemHeld;

        private HandState _handState;
        private float _stateTime;
        private float _punchCooldown;
        private WorldItem _removing;

        private enum HandState
        {
            Idle,
            Throwing,
            RemovingItem,
        }

        public bool UpdateHand(ButtonControl button, float multiplier)
        {
            if (_punchCooldown > 0)
            {
                _punchCooldown -= Time.deltaTime;
            }

            if (_handState == HandState.Throwing || _handState == HandState.RemovingItem)
            {
                var max = _handState == HandState.Throwing
                    ? manager.ThrowMultiplierCurve.keys[manager.ThrowMultiplierCurve.length - 1].time
                    : manager.TimeToRemoveItemFromTree;

                _stateTime += Time.deltaTime;
                manager.InteractionProgressSlider.value = Mathf.Clamp01(_stateTime / max);
            }

            var buttonWasPressedThisFrame = button.wasPressedThisFrame && !OnScreenUIManager.Instance.ShouldLockInput;
            var buttonWasReleasedThisFrame = button.wasReleasedThisFrame;

            if (itemHeld == null)
            {
                if (_handState == HandState.RemovingItem && _stateTime >= manager.TimeToRemoveItemFromTree)
                {
                    manager.PickupItemToHand(_removing, isLeftHand);
                    _handState = HandState.Idle;
                    _removing = null;
                }

                if (buttonWasPressedThisFrame)
                {
                    var didHit = Physics.Raycast(manager.Head.position, manager.FpCam.forward, out var hit,
                        manager.Range);
                    if (!didHit)
                    {
                        if (manager.InteractableAreas.TryPeek(out var area))
                        {
                            area.Interact(NetworkObject, isLeftHand);
                            return false;
                        }

                        Punch(false, hit, multiplier);
                        return true;
                    }

                    if (!hit.transform.TryGetComponent(out WorldItem item))
                    {
                        if (hit.transform.TryGetComponent(out IInteractable interactable))
                        {
                            if (interactable.Interact(NetworkObject, isLeftHand)) return false;
                        }

                        Punch(true, hit, multiplier);
                        return true;
                    }

                    if (item.IsOnTree && _handState != HandState.RemovingItem)
                    {
                        _handState = HandState.RemovingItem;
                        _stateTime = 0;
                        _removing = item;
                    }
                    else
                    {
                        manager.PickupItemToHand(item, isLeftHand);
                    }
                }

                if (buttonWasReleasedThisFrame && _handState == HandState.RemovingItem)
                {
                    _handState = HandState.Idle;
                }
            }
            else
            {
                if (buttonWasPressedThisFrame)
                {
                    var didHit = Physics.Raycast(manager.Head.position, manager.FpCam.forward, out var hit,
                        manager.Range);
                    if (didHit && hit.transform.TryGetComponent(out IItemTaker interactable))
                    {
                        if (interactable.Submit(itemHeld))
                        {
                            itemHeld = null;
                        }

                        return false;
                    }
                }

                if (buttonWasPressedThisFrame && _handState != HandState.Throwing)
                {
                    _handState = HandState.Throwing;
                    _stateTime = 0;
                }

                if (buttonWasReleasedThisFrame && _handState == HandState.Throwing)
                {
                    _handState = HandState.Idle;
                    if (itemHeld.Drop())
                    {
                        if (itemHeld.TryGetComponent(out Rigidbody rb))
                        {
                            rb.AddForce(
                                (manager.FpCam.forward + new Vector3(0, 0.4f, 0)) *
                                (manager.ThrowMultiplierCurve.Evaluate(_stateTime) * multiplier * GetComponent<Rigidbody>().linearVelocity.magnitude),
                                ForceMode.VelocityChange
                            );
                        }

                        itemHeld = null;
                        return true;
                    }
                }
            }

            return false;
        }

        public void PickupItem(WorldItem item)
        {
            if (!item.PickUp(manager.Inventory, position)) return;
            itemHeld = item;
        }

        private void Punch(bool didHit, RaycastHit hit, float multiplier)
        {
            if (!didHit) return;
            if (_punchCooldown > 0) return;
            if (!hit.transform.TryGetComponent(out PlayerHandManager no)) return;
            manager.ApplyForceRpc(no, transform.forward * (manager.ShoveForce * multiplier),
                RpcTarget.Single(no.OwnerClientId, RpcTargetUse.Temp));
            _punchCooldown = manager.ShoveCooldown;
        }
    }
}