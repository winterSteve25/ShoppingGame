using System;
using Player;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class SuperStrengthPowerUp : MonoBehaviour, IPowerUpEffect
    {
        [SerializeField] private float multiplier;
        
        public void Apply(PlayerCharacterController player)
        {
            if (player.TryGetComponent(out PlayerHandManager handMan))
            {
                handMan.MultiplySuperStrengthMultiplierRpc(multiplier);
            }
        }

        public Action<PlayerCharacterController> RemoveEffect()
        {
            return p =>
            {
                if (p.TryGetComponent(out PlayerHandManager handMan))
                {
                    handMan.MultiplySuperStrengthMultiplierRpc(1 / multiplier);
                }
            };
        }
    }
}