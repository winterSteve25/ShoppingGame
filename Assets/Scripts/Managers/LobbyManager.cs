using Cysharp.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Singleton { get; private set; }

        public Lobby? Lobby { get; private set; }

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public async UniTask<bool> CreateLobby()
        {
            var lobby = await SteamMatchmaking.CreateLobbyAsync(8);
            if (!lobby.HasValue)
            {
                Debug.LogError("Failed to create steam lobby");
                return false;
            }

            lobby.Value.SetPublic();
            Lobby = lobby;
            return true;
        }

        public async UniTask<bool> JoinLobby(Lobby lobby)
        {
            var result = await lobby.Join();
            if (result == RoomEnter.Success)
            {
                Lobby = lobby;
                return true;
            }

            Debug.LogError("Failed to join lobby: " + result);
            return false;
        }

        public void LeaveLobby()
        {
            if (Lobby == null)
            {
                Debug.LogWarning("Tried to leave lobby without lobby");
                return;
            }

            Lobby.Value.Leave();
            Lobby = null;
        }
    }
}