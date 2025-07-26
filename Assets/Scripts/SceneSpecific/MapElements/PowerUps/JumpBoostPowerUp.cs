using System;
using Player;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class JumpBoostPowerUp : MonoBehaviour, IPowerUpEffect
    {
        [SerializeField] private float multiplier;
        
        public void Apply(PlayerCharacterController player)
        {
            player.MultiplyJumpSpeedRpc(multiplier);
        }

        public Action<PlayerCharacterController> RemoveEffect()
        {
            return p => p.MultiplyJumpSpeedRpc(1/multiplier);
        }
    }
}