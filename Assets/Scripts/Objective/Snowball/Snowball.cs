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
            
            if (other.gameObject.CompareTag("Player") &&
                other.gameObject.TryGetComponent(out PlayerHandInteraction hand))
            {
                hand.ApplyForceRpc(hand,
                    rb.linearVelocity.normalized * force * forceMultiplierOnPlayer,
                    RpcTarget.Single(hand.OwnerClientId, RpcTargetUse.Temp));
            }
            else if (other.gameObject.TryGetComponent(out NetworkRigidbody networkRigidbody))
            {
                ApplyForceToRpc(networkRigidbody,
                    rb.linearVelocity.normalized * force,
                    RpcTarget.Single(networkRigidbody.OwnerClientId, RpcTargetUse.Temp));
            }

            RequestDespawnRpc();
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplyForceToRpc(NetworkBehaviourReference target, Vector3 force, RpcParams _)
        {
            if (!target.TryGet(out NetworkRigidbody rb)) return;
            rb.Rigidbody.AddForce(
                force,
                ForceMode.VelocityChange
            );
        }

        [Rpc(SendTo.Server)]
        private void RequestDespawnRpc()
        {
            NetworkObject.Despawn();
        }
    }
}