using Player;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.Lobby
{
    public class TeamPlatforms : NetworkBehaviour
    {
        [SerializeField] private StartGameManager startGameManager;
        [SerializeField] private byte team;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (!other.CompareTag("Player")) return;
            if (!other.gameObject.TryGetComponent(out PlayerIdentity player)) return;
            
            startGameManager.AddToTeam(player.ClientId, team);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;
            if (!other.CompareTag("Player")) return;
            if (!other.gameObject.TryGetComponent(out PlayerIdentity player)) return;
            
            startGameManager.RemoveFromTeam(player.ClientId, team);
        }
    }
}