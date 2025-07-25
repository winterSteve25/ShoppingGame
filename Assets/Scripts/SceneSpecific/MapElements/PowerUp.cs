using System;
using Cysharp.Threading.Tasks;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class PowerUp : NetworkBehaviour
    {
        private IPowerUpEffect _powerUpEffect;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (!other.TryGetComponent(out PlayerCharacterController player)) return;

            _powerUpEffect.Apply(player);
            RemoveEffect(player, _powerUpEffect.EffectDuration, _powerUpEffect.RemoveEffect())
                .Forget();
            
            NetworkObject.Despawn();
        }

        private async UniTaskVoid RemoveEffect(PlayerCharacterController player,
            float effectDuration,
            Action<PlayerCharacterController> removeEffect)
        {
            await UniTask.Delay((int)(effectDuration * 1000));
            removeEffect(player);
        }
    }
}