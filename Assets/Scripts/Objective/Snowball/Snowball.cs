using Items;
using Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Objective.Snowball
{
    public class Snowball : WorldItem
    {
        [SerializeField] private float force;
        [SerializeField] private float forceMultiplierOnPlayer;

        private void OnCollisionEnter(Collision other)
        {
            if (!IsOwner) return;
            
            var direction = rb.linearVelocity;
            direction.y = 0;
            direction.Normalize();
            direction *= force * rb.linearVelocity.magnitude * 0.1f;
            
            if (other.gameObject.CompareTag("Player") &&
                other.gameObject.TryGetComponent(out PlayerHandManager hand))
            {
                hand.ApplyForceRpc(hand,
                    direction * forceMultiplierOnPlayer,
                    RpcTarget.Single(hand.OwnerClientId, RpcTargetUse.Temp));
            }
            else if (other.gameObject.TryGetComponent(out NetworkRigidbody networkRigidbody))
            {
                ApplyForceToRpc(networkRigidbody,
                    direction,
                    RpcTarget.Single(networkRigidbody.OwnerClientId, RpcTargetUse.Temp));
            }

            RequestDespawnRpc();
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplyForceToRpc(NetworkBehaviourReference target, Vector3 force, RpcParams _)
        {
            if (!target.TryGet(out NetworkRigidbody rb)) return;
            rb.Rigidbody.linearVelocity = force;
        }

        [Rpc(SendTo.Server)]
        private void RequestDespawnRpc()
        {
            NetworkObject.Despawn();
        }
    }
}