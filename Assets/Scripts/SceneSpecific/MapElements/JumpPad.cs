using Player;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class JumpPad : NetworkBehaviour
    {
        [SerializeField] private float upwardForce;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (other.gameObject.TryGetComponent(out PlayerHandManager player))
            {
                player.ApplyForceRpc(player, new Vector3(0, upwardForce, 0),
                    RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
            }
            else
            {
                other.attachedRigidbody.AddForce(
                    new Vector3(0, upwardForce, 0),
                    ForceMode.VelocityChange
                );
            }
        }
    }
}