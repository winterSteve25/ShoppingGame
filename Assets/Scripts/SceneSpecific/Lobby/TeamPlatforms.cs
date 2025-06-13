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
            startGameManager.AddToTeam(NetworkManager.LocalClientId, team);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;
            startGameManager.RemoveFromTeam(NetworkManager.LocalClientId, team);
        }
    }
}