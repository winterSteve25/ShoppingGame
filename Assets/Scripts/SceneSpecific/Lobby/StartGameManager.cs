using System.Collections.Generic;
using System.Linq;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSpecific.Lobby
{
    // ONLY DOES STUFF ON SERVER
    public class StartGameManager : NetworkBehaviour
    {
        private Dictionary<byte, List<ulong>> _teams;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _teams = new Dictionary<byte, List<ulong>>();
        }

        public void AddToTeam(ulong player, byte team)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Can not team modify on client");
                return;
            }
            
            if (!_teams.ContainsKey(team))
            {
                _teams.Add(team, new List<ulong>());
            }
            
            _teams[team].Add(player);
            CheckStart();
        }

        public void RemoveFromTeam(ulong player, byte team)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Can not modify team on client");
                return;
            }
            
            if (!_teams.ContainsKey(team))
            {
                Debug.LogWarning($"Tried to remove {player} from team {team} but team {team} doesnt exist");
                return;
            }
            
            _teams[team].Remove(player);
        }

        private void CheckStart()
        {
            var numPlayersReady = _teams.Aggregate(0, (acc, team) => acc + team.Value.Count);

            if (NetworkManager.ConnectedClients.Count == numPlayersReady &&
                LobbyManager.Singleton.Lobby != null &&
                numPlayersReady == LobbyManager.Singleton.Lobby.Value.MemberCount)
            {
                NetworkManager.SceneManager.LoadScene("Scenes/Game Scene", LoadSceneMode.Single);
            }
        }
    }
}