using Cysharp.Threading.Tasks;
using Managers;
using Netcode.Transports.Facepunch;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace SceneSpecific.MainMenu
{
    public class JoinLobbyButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;

        private Friend _friend;

        public void Init(Friend friend)
        {
            _friend = friend;
            text.text = friend.Name;

            UniTask.Void(async () =>
            {
                var ava = await friend.GetMediumAvatarAsync();
                if (!ava.HasValue) return;
                image.sprite = ava.Value.ToTexture2D().ToSprite(new Vector2(0.5f, 0.5f));
            });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var info = _friend.GameInfo;
            if (!info.HasValue) return;

            var lobby = info.Value.Lobby;
            if (!lobby.HasValue) return;

            UniTask.Void(async () =>
            {
                var result = await LobbyManager.Singleton.JoinLobby(lobby.Value);
                if (!result) return;

                await SceneManager.LoadSceneAsync("Scenes/Lobby Scene");
                NetworkManager.Singleton.GetComponent<FacepunchTransport>()
                    .targetSteamId = lobby.Value.Owner.Id;
                NetworkManager.Singleton.StartClient();
            });
        }
    }
}