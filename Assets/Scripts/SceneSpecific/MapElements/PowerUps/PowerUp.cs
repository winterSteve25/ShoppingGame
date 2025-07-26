using System;
using Cysharp.Threading.Tasks;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class PowerUp : NetworkBehaviour
    {
        [SerializeField] private float effectDuration;

        private IPowerUpEffect _powerUpEffect;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _powerUpEffect = GetComponent<IPowerUpEffect>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (!other.TryGetComponent(out PlayerCharacterController player)) return;

            _powerUpEffect.Apply(player);

            if (effectDuration >= 0)
            {
                RemoveEffect(player, effectDuration, _powerUpEffect.RemoveEffect())
                    .Forget();
            }

            NetworkObject.Despawn();
        }

        private async UniTaskVoid RemoveEffect(PlayerCharacterController player,
            float effectDuration,
            Action<PlayerCharacterController> removeEffect)
        {
            if (effectDuration > 0)
            {
                await UniTask.Delay((int)(effectDuration * 1000));
            }

            removeEffect(player);
        }
    }
}