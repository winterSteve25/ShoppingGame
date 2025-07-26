using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.MapElements
{
    public class PowerUpSpawner : NetworkBehaviour
    {
        [SerializeField] private float time;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private NetworkObject[] prefabs;

        private float _timer;

        private void Update()
        {
            if (!IsServer) return;
            _timer += Time.deltaTime;

            if (_timer >= time)
            {
                var obj = prefabs[Random.Range(0, prefabs.Length)];
                NetworkManager
                    .Singleton.SpawnManager
                    .InstantiateAndSpawn(obj, position: spawnPoints[Random.Range(0, spawnPoints.Length)].position);

                _timer = 0;
            }
        }
    }
}