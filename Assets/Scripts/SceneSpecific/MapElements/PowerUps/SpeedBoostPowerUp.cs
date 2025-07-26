using System;
using Player;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class SpeedBoostPowerUp : MonoBehaviour, IPowerUpEffect
    {
        [SerializeField] private float multiplier;

        public void Apply(PlayerCharacterController player)
        {
            player.MultiplyMovementSpeedRpc(multiplier);
        }

        public Action<PlayerCharacterController> RemoveEffect()
        {
            return p => p.MultiplyMovementSpeedRpc(1 / multiplier);
        }
    }
}