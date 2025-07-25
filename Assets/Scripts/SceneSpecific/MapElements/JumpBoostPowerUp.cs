using System;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class JumpBoostPowerUp : NetworkBehaviour, IPowerUpEffect
    {
        [SerializeField] private float multiplier;
        
        public float EffectDuration => 5;

        public void Apply(PlayerCharacterController player)
        {
            player.jumpUpSpeed *= multiplier;
        }

        public Action<PlayerCharacterController> RemoveEffect()
        {
            return p => p.jumpUpSpeed /= multiplier;
        }
    }
}