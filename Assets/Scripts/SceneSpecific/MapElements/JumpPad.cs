using Player;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class JumpPad : NetworkBehaviour
    {
        [SerializeField] private float upwardForce;
        [SerializeField] private float upwardForceItem;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (other.gameObject.TryGetComponent(out PlayerHandManager player))
            {
                player.ApplyForceRpc(player, new Vector3(0, upwardForce, 0),
                    false,
                    RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
            }
            else
            {
                other.attachedRigidbody.AddForce(
                    new Vector3(0, upwardForceItem, 0),
                    ForceMode.VelocityChange
                );
            }
        }
    }
}