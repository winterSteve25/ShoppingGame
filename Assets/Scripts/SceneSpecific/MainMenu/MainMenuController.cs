using Cysharp.Threading.Tasks;
using Managers;
using Netcode.Transports.Facepunch;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSpecific.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private FacepunchTransport transport;

        public void CreateLobby()
        {
            UniTask.Void(async () =>
            {
                if (!await LobbyManager.Singleton.CreateLobby()) return;
                transport.targetSteamId = SteamClient.SteamId;
                await SceneManager.LoadSceneAsync("Scenes/Lobby Scene");
                NetworkManager.Singleton.StartHost();
            });
        }

        public void JoinLobby()
        {
            SceneManager.LoadScene("Scenes/Join Lobby");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}