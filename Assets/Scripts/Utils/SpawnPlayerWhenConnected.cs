using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    public class SpawnPlayerWhenConnected : NetworkBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Transform spawnPoint;
        
        private void Start()
        {
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, NetworkManager.LocalClientId, isPlayerObject: true);
            obj.GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint.position);
        }
    }
}